using Avalonia.Controls;
using Avalonia.Threading;
using iControl.Desktop.Core.Interfaces;
using iControl.Desktop.XPlat.Controls;
using iControl.Desktop.XPlat.ViewModels;
using iControl.Desktop.XPlat.Views;
using iControl.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iControl.Desktop.XPlat.Services
{
    public class ChatUiServiceLinux : IChatUiService
    {
        private ChatWindowViewModel ChatViewModel { get; set; }

        public event EventHandler ChatWindowClosed;

        public void ReceiveChat(ChatMessage chatMessage)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (chatMessage.Disconnected)
                {
                    await MessageBox.Show("The partner has disconnected.", "Partner Disconnected", MessageBoxType.OK);
                    Environment.Exit(0);
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
            Dispatcher.UIThread.Post(() =>
            {
                var chatWindow = new ChatWindow();
                chatWindow.Closing += ChatWindow_Closing;
                ChatViewModel = chatWindow.DataContext as ChatWindowViewModel;
                ChatViewModel.PipeStreamWriter = writer;
                ChatViewModel.OrganizationName = organizationName;
                App.Current.Run(chatWindow);
            });
        }

        private void ChatWindow_Closing(object sender, CancelEventArgs e)
        {
            ChatWindowClosed?.Invoke(this, null);
        }
    }
}
