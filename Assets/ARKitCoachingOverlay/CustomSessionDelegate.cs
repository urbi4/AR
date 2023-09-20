#if UNITY_IOS
using UnityEngine.XR.ARKit;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class CustomSessionDelegate : DefaultARKitSessionDelegate
    {
        protected override void OnCoachingOverlayViewWillActivate(ARKitSessionSubsystem sessionSubsystem)
        {
            Debug.Log(nameof(OnCoachingOverlayViewWillActivate));
        }

        protected override void OnCoachingOverlayViewDidDeactivate(ARKitSessionSubsystem sessionSubsystem)
        {
            Debug.Log(nameof(OnCoachingOverlayViewDidDeactivate));
        }
    }
}
#endif
