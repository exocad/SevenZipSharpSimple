using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenZip.Specialized;

public class BulkExtractOptions
{
    public Func<ArchiveEntry, bool> Predicate { get; set; }

    public bool UseNativeStreamPool { get; set; }

    public int StreamPoolCapacity { get; set; }
}
