using Windows.ApplicationModel;

namespace Fluent_Video_Player.Helpers;

public static class Constants
{
    public static readonly string PlaylistTag = "playlist";

    public static readonly string ScaleUpValue = "1.03";
    public static readonly TimeSpan ScaleAnimationTime = TimeSpan.FromMilliseconds(500);
    public static readonly TimeSpan VolumeAppearTime = TimeSpan.FromMilliseconds(600);
    public static readonly TimeSpan VolumeDisappearTime = TimeSpan.FromMilliseconds(800);

    // TODO : Add AppCenter Key before publishing to store.
    public static readonly string AppCenterKey = "AddYourKeyHere";
    public static readonly string AppStoreId = "9p0jwpr9vn80";
    public static readonly string AppName = Package.Current.DisplayName;
    public static readonly string AppStoreLink = $"{AppName} {"InWindowsStore".GetLocalized()}";
    public static readonly TimeSpan ConnectedAnimationDuration = TimeSpan.FromSeconds(0.4);
    internal static readonly int TrainViewMaxItems = 20;

    public const string PanoramicStateName = "PanoramicState";
    public const string WideStateName = "WideState";
    public const string NarrowStateName = "NarrowState";
    public const double WideStateMinWindowWidth = 640;
    public const double PanoramicStateMinWindowWidth = 1024;

    public const uint _thumbnailReqestedSize = 190;
    public const uint _stepSize = 3;

    public const string MiniBarSettingsKey = "IsMiniBarShown";

    public const double SecondGridWidth = 260;// used in list mode only
    public const double SecondGridHeight = 180; // used in grid mode only.

    public const double PlayListDesiredWidth = 20000;
    public const double PlayListItemHeight = 80;

    public const double PlayGridDesiredWidth = 180;
    public const double PlayGridItemHeight = SecondGridHeight - (PlaylistHeaderHeight + 8);

    public const double PlaylistHeaderHeight = 40;
}
