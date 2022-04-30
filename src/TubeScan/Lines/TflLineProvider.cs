using TubeScan.Models;
using TubeScan.Tfl;

namespace TubeScan.Lines
{
    internal class TflLineProvider : LineProvider
    {
        private readonly ITflClient _tflClient;

        public TflLineProvider(ITflClient tflClient)
        {
            _tflClient = tflClient;
        }

        public override async Task<IList<LineStatus>> GetLineStatusAsync()
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
