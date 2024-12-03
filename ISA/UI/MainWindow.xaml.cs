using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Core;
using OpenTK.Audio.OpenAL.Extensions.Creative.EFX;
using ScottPlot;
using ScottPlot.Plottables;

namespace UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            dComboBox.Items.Add(0.1);
            dComboBox.Items.Add(0.01);
            dComboBox.Items.Add(0.001);
            dComboBox.Items.Add(0.0001);

            functionGoalComboBox.Items.Add("MAX");
            functionGoalComboBox.Items.Add("MIN");

            fLineEdit.Text = "mod(x,1) * (cos(20*pi*x) - sin(x))";
            aLineEdit.Text = "-4";
            bLineEdit.Text = "12";
            dComboBox.SelectedIndex = 2;
            functionGoalComboBox.SelectedIndex = 0;
            TLineEdit.Text = "30";

            fLineEdit.TextChanged += ValidateInputs;
            aLineEdit.TextChanged += ValidateInputs;
            bLineEdit.TextChanged += ValidateInputs;
            TLineEdit.TextChanged += ValidateInputs;
            dComboBox.SelectionChanged += ValidateInputs;
        }
        private static bool TryParseDouble(string input, out double n)
        {
            return double.TryParse(input.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out n);
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool isValid = TryParseDouble(aLineEdit.Text, out double a) &&
                           TryParseDouble(bLineEdit.Text, out double b) && a < b &&
                           int.TryParse(TLineEdit.Text, out int t) && t >= 0 &&
                           Utils.TryParseFunction(fLineEdit.Text, out var _) && 
                           dComboBox.SelectedItem != null;

            startButton.IsEnabled = isValid;
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _ = TryParseDouble(aLineEdit.Text, out double a);
            _ = TryParseDouble(bLineEdit.Text, out double b);
            double d = (double)dComboBox.SelectedItem;

            OptimizationGoal functionGoal = 
                functionGoalComboBox.SelectedIndex == 0 ? OptimizationGoal.Max : OptimizationGoal.Min;
            Func<double, double> f = Utils.ParseFunction(fLineEdit.Text);

            var inputs = new UserInputs(
                GenotypeSpace.FromD(d, a, b),
                T: int.Parse(TLineEdit.Text),
                f,
                functionGoal
            );
            var algo = new Algorithm(inputs);
            var xbin = algo.Run(out var stats);
            var xreal = Utils.Bin2Real(xbin, inputs.genotypeSpace);
            var row = new Row()
            {
                XReal = Math.Round(xreal, inputs.genotypeSpace.precision.decimalPlaces),
                XBin = xbin,
                Fx = Math.Round(f(xreal), inputs.genotypeSpace.precision.decimalPlaces),
            };
            var rows = new List<Row>() { row };
            DaneDataGrid.ItemsSource = rows;
            double rowHeight = 22;
            DaneDataGrid.MaxHeight = rowHeight * 20;
            // TODO: add chart
        }
        private class Row
        {
            public double XReal { get; set; }
            public string? XBin { get; set; }
            public double Fx { get; set; }
        }
    }
}
