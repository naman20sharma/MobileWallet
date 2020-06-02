using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyWallet.Views.Onboarding
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OnboardingPageV2 : ContentPage, IRootView
    {
        public OnboardingPageV2()
        {
            InitializeComponent();
        }
    }
}