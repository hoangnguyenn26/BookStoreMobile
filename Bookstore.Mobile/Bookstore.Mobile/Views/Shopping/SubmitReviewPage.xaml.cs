using Bookstore.Mobile.ViewModels;

namespace Bookstore.Mobile.Views;

public partial class SubmitReviewPage : ContentPage
{
	private readonly SubmitReviewViewModel _viewModel;
	public SubmitReviewPage(SubmitReviewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}
}