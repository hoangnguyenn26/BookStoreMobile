using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Bookstore.Mobile.ViewModels
{
    public partial class AdminReportsViewModel : BaseViewModel
    {
        private readonly IAdminReportApi _reportApi;
        private readonly ILogger<AdminReportsViewModel> _logger;

        public AdminReportsViewModel(IAdminReportApi reportApi, ILogger<AdminReportsViewModel> logger)
        {
            _reportApi = reportApi ?? throw new ArgumentNullException(nameof(reportApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Title = "Reports";
            EndDate = DateTime.Now.Date;
            StartDate = EndDate.AddDays(-6);
            RevenueReport = new RevenueReportDto();
            RevenueChart = new LineChart { Entries = new List<ChartEntry>() };
            BestsellersChart = new BarChart { Entries = new List<ChartEntry>() };
            BestsellersData = new List<BestsellerDto>();
            LowStockBooks = new ObservableCollection<LowStockBookDto>();
        }

        [ObservableProperty] private DateTime _startDate;
        [ObservableProperty] private DateTime _endDate;
        [ObservableProperty] private RevenueReportDto _revenueReport;
        [ObservableProperty] private Chart? _revenueChart;
        [ObservableProperty] private bool _showRevenueReport = false;
        [ObservableProperty] private Chart? _bestsellersChart;
        [ObservableProperty] private bool _showBestsellersReport = false;
        [ObservableProperty] private List<BestsellerDto> _bestsellersData;
        [ObservableProperty] private ObservableCollection<LowStockBookDto> _lowStockBooks;
        [ObservableProperty] private bool _showLowStockReport = false;
        [ObservableProperty] private int _lowStockThreshold = 5;

        [RelayCommand]
        private async Task LoadAllReportsAsync(bool isRefreshing = false)
        {
            await RunSafeAsync(async () =>
            {
                ShowRevenueReport = false;
                ShowBestsellersReport = false;
                ShowLowStockReport = false;
                var revenueTask = LoadRevenueReportInternalAsync();
                var bestsellersTask = LoadBestsellersReportInternalAsync();
                var lowStockTask = LoadLowStockReportInternalAsync();
                await Task.WhenAll(revenueTask, bestsellersTask, lowStockTask);
            }, showBusy: true);
        }


        private async Task LoadRevenueReportInternalAsync()
        {
            try
            {
                var inclusiveEndDate = EndDate.Date.AddDays(1);
                var inclusiveStartDate = StartDate.Date;
                _logger.LogInformation("Loading revenue report from {StartDate} to {EndDate}", inclusiveStartDate, EndDate.Date);
                var response = await _reportApi.GetRevenueReport(inclusiveStartDate, EndDate.Date);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RevenueReport = response.Content;
                        CreateRevenueChart();
                        ShowRevenueReport = true;
                        _logger.LogInformation("Revenue report loaded successfully.");
                    });
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed";
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = (ErrorMessage ?? "") + $"\nRevenue Error: {errorContent}";
                    });
                    _logger.LogWarning("Failed to load revenue report. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception loading revenue report.");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ErrorMessage = (ErrorMessage ?? "") + $"\nRevenue Error: {ex.Message}";
                });
            }
        }

        private void CreateRevenueChart()
        {
            if (RevenueReport?.DailyRevenue == null || !RevenueReport.DailyRevenue.Any())
            {
                RevenueChart = new LineChart { Entries = new List<ChartEntry>(), LabelTextSize = 30f, LabelOrientation = Orientation.Horizontal };
                return;
            }
            var entries = new List<ChartEntry>();
            SKColor entryColor = SKColor.Parse("#2196F3");
            foreach (var dailyData in RevenueReport.DailyRevenue)
            {
                entries.Add(new ChartEntry((float)dailyData.TotalRevenue)
                {
                    Label = dailyData.Date.ToString("dd/MM"),
                    ValueLabel = dailyData.TotalRevenue.ToString("N0"),
                    Color = entryColor
                });
            }
            RevenueChart = new LineChart { Entries = entries, LineMode = LineMode.Spline, PointMode = PointMode.Circle, PointSize = 10f, LabelTextSize = 30f, LabelOrientation = Orientation.Horizontal, ValueLabelOrientation = Orientation.Horizontal, ValueLabelTextSize = 25f, MinValue = 0 };
            OnPropertyChanged(nameof(RevenueChart)); // Notify UI
        }


        private async Task LoadBestsellersReportInternalAsync()
        {
            try
            {
                var inclusiveEndDate = EndDate.Date.AddDays(1);
                var inclusiveStartDate = StartDate.Date;
                _logger.LogInformation("Loading bestsellers report from {StartDate} to {EndDate}", inclusiveStartDate, EndDate.Date);
                var response = await _reportApi.GetBestsellersReport(inclusiveStartDate, EndDate.Date, top: 7);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        BestsellersData = response.Content.ToList();
                        CreateBestsellersChart();
                        ShowBestsellersReport = BestsellersData.Any();
                        _logger.LogInformation("Bestsellers report loaded successfully.");
                    });
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed";
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = (ErrorMessage ?? "") + $"\nBestsellers Error: {errorContent}";
                    });
                    _logger.LogWarning("Failed to load bestsellers. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception loading bestsellers report.");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ErrorMessage = (ErrorMessage ?? "") + $"\nBestsellers Error: {ex.Message}";
                });
            }
        }

        private void CreateBestsellersChart()
        {
            if (BestsellersData == null || !BestsellersData.Any())
            {
                BestsellersChart = new BarChart { Entries = new List<ChartEntry>(), LabelTextSize = 25f };
                return;
            }
            var entries = new List<ChartEntry>();
            SKColor entryColor = SKColor.Parse("#4CAF50");
            foreach (var book in BestsellersData.OrderByDescending(b => b.TotalQuantitySold))
            {
                entries.Add(new ChartEntry(book.TotalQuantitySold)
                {
                    Label = book.BookTitle.Length > 15 ? book.BookTitle.Substring(0, 12) + "..." : book.BookTitle,
                    ValueLabel = book.TotalQuantitySold.ToString(),
                    Color = entryColor,
                    ValueLabelColor = entryColor.WithAlpha(200)
                });
            }
            BestsellersChart = new BarChart { Entries = entries, LabelTextSize = 25f, ValueLabelOrientation = Orientation.Horizontal, LabelOrientation = Orientation.Horizontal, IsAnimated = true };
            OnPropertyChanged(nameof(BestsellersChart)); // Notify UI
        }


        private async Task LoadLowStockReportInternalAsync()
        {
            try
            {
                _logger.LogInformation("Loading low stock report with threshold {Threshold}", LowStockThreshold);
                var response = await _reportApi.GetLowStockReport(LowStockThreshold);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LowStockBooks.Clear();
                        foreach (var book in response.Content) LowStockBooks.Add(book);
                        ShowLowStockReport = LowStockBooks.Any();
                        _logger.LogInformation("Low stock report loaded successfully. Found {Count} items.", LowStockBooks.Count);
                    });
                }
                else
                {
                    string errorContent = response.Error?.Content ?? response.ReasonPhrase ?? "Failed";
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ErrorMessage = (ErrorMessage ?? "") + $"\nLow Stock Error: {errorContent}";
                    });
                    _logger.LogWarning("Failed to load low stock. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception loading low stock report.");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ErrorMessage = (ErrorMessage ?? "") + $"\nLow Stock Error: {ex.Message}";
                });
            }
        }

        [RelayCommand]
        private async Task RefreshAllReports() => await LoadAllReportsAsync(true);

        [RelayCommand]
        private async Task ReloadLowStock() => await LoadLowStockReportInternalAsync(); // Command for reload button


        public void OnAppearing()
        {
            if (!ShowRevenueReport || !ShowBestsellersReport || !ShowLowStockReport)
            {
                LoadAllReportsCommand.Execute(false);
            }
        }
    }
}