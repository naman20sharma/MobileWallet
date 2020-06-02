using System.Collections.Generic;
using ZXing.Mobile;

namespace MyWallet.Services
{
    public interface IZXingHelper
    {
        CameraResolution CameraResolutionSelectorDelegateImplementation(List<CameraResolution> availableresolutions);
    }
}
