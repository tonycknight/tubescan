using Tk.Extensions.Guards;
using Tk.Extensions.Tasks;
using TubeScan.Models;

namespace TubeScan.Lines
{
    internal class LineProvider : ILineProvider, ISettable<LineStatus>
    {
        private readonly ILineReferenceProvider _linesProvider;
        private readonly ILineStatusProvider _lineStatusProvider;
        private IList<LineStatus> _lastKnownLineStatuses;

        public LineProvider(ILineReferenceProvider linesProvider, ILineStatusProvider lineStatusProvider)
        {            
            _linesProvider = linesProvider;
            _lineStatusProvider = lineStatusProvider;
        }

        public Task<IList<Line>> GetLinesAsync() => _linesProvider.GetLines().ToTaskResult();

        public Task<IList<LineStatus>> GetLineStatusAsync() => _lastKnownLineStatuses?.ToTaskResult() ?? _lineStatusProvider.GetLineStatusAsync();

        void ISettable<LineStatus>.Set(IList<LineStatus> values)
        {
            _lastKnownLineStatuses = values.ArgNotNull(nameof(values));
        }
    }
}
