using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;

        private bool highlightLastChange = false;  // Поле для збереження стану налаштування

        // Додаємо обробники для нового чекбокса
        public SettingsWindow(MainWindow owner)
        {
            InitializeComponent();
            mainWindow = owner;

            this.Left = mainWindow.Left + mainWindow.Width + 10;
            this.Top = mainWindow.Top;

            FixSettingCheckBox.Checked += FixSettingCheckBox_Checked;
            FixSettingCheckBox.Unchecked += FixSettingCheckBox_Unchecked;

            NightModeCheckBox.Checked += NightModeCheckBox_Checked;
            NightModeCheckBox.Unchecked += NightModeCheckBox_Unchecked;

            HighlightErrorsCheckBox.Checked += HighlightErrorsCheckBox_Checked;
            HighlightErrorsCheckBox.Unchecked += HighlightErrorsCheckBox_Unchecked;

            SettingCheckBox1.Checked += HighlightRowColumnCheckBox_Checked;
            SettingCheckBox1.Unchecked += HighlightRowColumnCheckBox_Unchecked;

            LastChangeCheckBox.Checked += LastChangeCheckBox_Checked;
            LastChangeCheckBox.Unchecked += LastChangeCheckBox_Unchecked;

        }

        private void LastChangeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableLastChangeHighlighting(true);  // Передаємо це налаштування в головне вікно
        }

        private void LastChangeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableLastChangeHighlighting(false);
        }

        private void HighlightErrorsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableErrorHighlighting(true);  // Увімкнено виділення помилок
        }

        private void HighlightErrorsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableErrorHighlighting(false);  // Вимкнено виділення помилок
        }



        private void FixSettingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }

        private void FixSettingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            this.Owner?.Activate();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            if (!FixSettingCheckBox.IsChecked ?? false)
            {
                this.Topmost = false;
            }
        }

        private void HighlightRowColumnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableRowColumnHighlighting(true);
        }

        private void HighlightRowColumnCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableRowColumnHighlighting(false);
        }


        private void NightModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableNightMode(true);
            this.Background = new SolidColorBrush(Colors.DarkSlateGray);
        }

        private void NightModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mainWindow.EnableNightMode(false); 
            this.Background = new SolidColorBrush(Colors.White);

        }
    }
}
