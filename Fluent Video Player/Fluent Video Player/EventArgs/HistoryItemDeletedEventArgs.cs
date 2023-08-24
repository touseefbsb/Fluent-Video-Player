using Fluent_Video_Player.Models;

namespace Fluent_Video_Player.EventArgs;

public class HistoryItemDeletedEventArgs : System.EventArgs
{
    public Video MyVideo { get; set; }
}
