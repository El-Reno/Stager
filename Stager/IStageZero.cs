using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Stager
{
    public interface IStageZero
    {
        Task<StagerCommand> RequestCommand(Uri site);
        Task<byte[]> RequestStage(Uri site);
        int LoadUriList(FileInfo uriFile);
        void AddUrisToList(List<Uri> addUris);
        void RemoveUrisFromList(List<Uri> removeUris);
        int LoadStage(byte[] assembly, Dictionary<string, string> arguments);
    }
}
