using Tk.Extensions.Tasks;
using TubeScan.Models;

namespace TubeScan.Lines
{
    internal class LineProvider : ILineProvider
    {
        private readonly ILineReferenceProvider _linesProvider;
        private readonly ILineStatusProvider _lineStatusProvider;

        public LineProvider(ILineReferenceProvider linesProvider, ILineStatusProvider lineStatusProvider)
        {            
            _linesProvider = linesProvider;
            _lineStatusProvider = lineStatusProvider;
        }

        public Task<IList<Line>> GetLinesAsync() => _linesProvider.GetLines().ToTaskResult();

        public Task<IList<LineStatus>> GetLineStatusAsync() => _lineStatusProvider.GetLineStatusAsync();
    }
}
