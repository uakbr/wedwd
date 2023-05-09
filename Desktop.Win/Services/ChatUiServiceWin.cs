﻿using iControl.Desktop.Core.Interfaces;
using iControl.Desktop.Win.ViewModels;
using iControl.Desktop.Win.Views;
using iControl.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iControl.Desktop.Win.Services
{
    public class ChatUiServiceWin : IChatUiService
    {
        private ChatWindowViewModel ChatViewModel { get; set; }

        public event EventHandler ChatWindowClosed;

        public void ReceiveChat(ChatMessage chatMessage)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (chatMessage.Disconnected)
                {
                    MessageBox.Show("Your partner has disconnected.", "Partner Disconnected", MessageBoxButton.OK, MessageBoxImage.Information);
                    App.Current.Shutdown();
                    return;
                }

                if (ChatViewModel != null)
                {
                    ChatViewModel.SenderName = chatMessage.SenderName;
                    ChatViewModel.ChatMessages.Add(chatMessage);
                }
            });
        }

        public void ShowChatWindow(string organizationName, StreamWriter writer)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var chatWindow = new ChatWindow();
                chatWindow.Closing += ChatWindow_Closing;
                ChatViewModel = chatWindow.DataContext as ChatWindowViewModel;
                ChatViewModel.PipeStreamWriter = writer;
                ChatViewModel.OrganizationName = organizationName;
                chatWindow.Show();
            });
        }

        private void ChatWindow_Closing(object sender, CancelEventArgs e)
        {
            ChatWindowClosed?.Invoke(this, null);
        }
    }
}
