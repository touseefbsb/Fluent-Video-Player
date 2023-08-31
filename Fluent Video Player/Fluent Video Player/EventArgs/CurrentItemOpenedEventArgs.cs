namespace Fluent_Video_Player.EventArgs;

public class CurrentItemOpenedEventArgs : System.EventArgs
{
    public string CurrentTitle { get; set; } = "";
    public TimeSpan ResumePosition { get; set; } = TimeSpan.Zero;
}
