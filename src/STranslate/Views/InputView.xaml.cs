﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.Log;
using STranslate.Model;
using STranslate.ViewModels;

namespace STranslate.Views;

public partial class InputView
{
    /// <summary>
    ///     由于windows设定有高精度/超高精度触摸板区别，和鼠标表现不一致，所以使用固定offset
    /// </summary>
    private const double Offset = 30;

    public InputView()
    {
        InitializeComponent();
    }

    private void InputTB_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var textBox = (TextBox)sender;

        // 检查是否按住 Ctrl 键，如果按住则进行缩放等特殊操作，否则进行滚动
        var currentFontSize = (double)Application.Current.Resources[Constant.FontSize18TextBox];
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // 根据滚轮事件更改字体大小
            if (e.Delta > 0)
            {
                if (currentFontSize < 40) currentFontSize += 2.0; // 根据需要调整放大的幅度
            }
            else
            {
                if (currentFontSize > 10) currentFontSize -= 2.0; // 根据需要调整放大的幅度
            }

            // 设置新的字体大小
            Application.Current.Resources[Constant.FontSize18TextBox] = currentFontSize;
        }
        else //修复普通滚动
        {
            if (e.Delta > 0)
            {
                if (textBox.VerticalOffset > 0) textBox.ScrollToVerticalOffset(textBox.VerticalOffset - Offset);
            }
            else
            {
                if (textBox.VerticalOffset < textBox.ExtentHeight - textBox.ViewportHeight)
                    textBox.ScrollToVerticalOffset(textBox.VerticalOffset + Offset);
            }
        }

        // 防止事件继续传播
        e.Handled = true;
    }

    /// <summary>
    ///     ListBox鼠标滚轮事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
        if (e.Handled || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) return;

        // ListBox拦截鼠标滚轮事件
        e.Handled = true;

        // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = sender
        };
        var parent = ((Control)sender).Parent as UIElement;
        parent!.RaiseEvent(eventArg);
    }

    private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            if (!Clipboard.ContainsText()) return;
            var clipboardText = Clipboard.GetText();
            var currentCaretIndex = InputTB.CaretIndex;

            if (InputTB.SelectionLength > 0)
            {
                // 如果有选中文本，直接替换
                InputTB.SelectedText = clipboardText;
            }
            else
            {
                // 如果没有选中文本，在当前光标位置插入
                // 通过 SelectedText 属性设置来支持撤销操作
                InputTB.Select(currentCaretIndex, 0);
                InputTB.SelectedText = clipboardText;
            }

            // 设置光标位置到插入文本的末尾
            InputTB.CaretIndex = currentCaretIndex + clipboardText.Length;


            ((InputViewModel)DataContext).TranslateCommand.Execute(null);
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"输入框粘贴文本失败 {ex.Message}", ex);
        }
    }
}