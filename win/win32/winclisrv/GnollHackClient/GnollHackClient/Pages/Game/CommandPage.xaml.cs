﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GnollHackCommon;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GnollHackClient.Pages.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommandPage : ContentPage
    {
        private GamePage _gamePage;
        public CommandPage(GamePage gamePage)
        {
            InitializeComponent();
            _gamePage = gamePage;
        }

        private async void DownButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '>');
        }

        private async void UpButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '<');
        }

        private async void CmdButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Meta('c'));
        }

        private async void SkillButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'S');
        }

        private async void AbilityButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'A');
        }

        private async void EatButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'e');
        }

        private async void LootButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'l');
        }

        private async void OpenButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'o');
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'c');
        }

        private async void YellButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Ctrl('y'));
        }

        private async void ChatButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'C');
        }

        private async void TwoWeapButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Ctrl('x'));
        }

        private async void SwapWeapButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'x');
        }

        private async void SpellsButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '+');
        }

        private async void PrayButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Meta('p'));
        }

        private async void SacrificeButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Meta('o'));
        }
        private async void NameButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Meta('N'));
        }
        private async void QuaffButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'q');
        }
        private async void PayButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'p');
        }
        private async void DigButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Ctrl('g'));
        }
        private async void ForceFightButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'F');
        }
        private async void LightButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, GHUtils.Ctrl('l'));
        }
        private async void TravelButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '_');
        }
        private async void ExtendedCommandButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '#');
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
        }

        private async void WhatIsButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '/');
        }

        private async void LookFarButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, ';');
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, '?');
        }

        private async void EngraveButton_Clicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
            _gamePage.GenericButton_Clicked(sender, e, 'E');
        }
    }
}