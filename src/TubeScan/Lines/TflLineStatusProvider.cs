using Tk.Extensions.Tasks;
using TubeScan.Models;
using TubeScan.Tfl;

namespace TubeScan.Lines
{
    internal class TflLineStatusProvider : ILineStatusProvider
    {
        private readonly ITflClient _tflClient;

        public TflLineStatusProvider(ITflClient tflClient)
        {
            _tflClient = tflClient;
        }

        public async Task<IList<LineStatus>> GetLineStatusAsync()
        {
            var resp = await _tflClient.GetAsync("Line/Mode/tube/Status?detail=true", false);
            if (!resp.IsSuccess)
            {
                throw new ApplicationException($"Bad response from TFL: {resp.HttpStatus} received.");
            }

            return resp.Body.ToLineStatuses();
        }
    }
}
