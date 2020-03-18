using ModMyFactory.WebApi;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class MessageHelper
    {
        public static async Task ShowMessageForApiException(ApiException exception)
        {
            if (exception is ConnectFailureException)
            {
                // Connection error
                await Messages.ConnectionError.Show(exception);
            }
            else if (exception is TimeoutException)
            {
                // Timeout
                await Messages.TimeoutError.Show(exception);
            }
            else
            {
                // Server error
                await Messages.ServerError.Show(exception);
            }
        }
    }
}
