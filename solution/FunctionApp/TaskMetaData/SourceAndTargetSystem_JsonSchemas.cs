using AdsGoFast.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdsGoFast
{
    class SourceAndTargetSystem_JsonSchemas:IEnumerable<SourceAndTargetSystem_JsonSchema>, IDisposable
    {
        public SourceAndTargetSystem_JsonSchemas()
        {
            TaskMetaDataDatabase TMD = new AdsGoFast.TaskMetaDataDatabase();
            _internallist = TMD.GetSqlConnection().QueryWithRetry<SourceAndTargetSystem_JsonSchema>("select * from [dbo].[SourceAndTargetSystems_JsonSchema]").ToList();
        }
        private List<SourceAndTargetSystem_JsonSchema> _internallist { get; set; }

        public SourceAndTargetSystem_JsonSchema GetBySystemType(string SystemType)
        {
            SourceAndTargetSystem_JsonSchema ret;
            if (this._internallist.Where(x => x.SystemType == SystemType).Any())
            {
                ret = this._internallist.Where(x => x.SystemType == SystemType).First();
            }
            else
            {
                throw (new Exception("Failed to find SourceAndTargetSystems_JsonSchema record for SystemType: " + SystemType));
            }

            return ret;
        }

        public void Dispose()
        {
            _internallist.Clear();
            _internallist = null;
        }

        public IEnumerator<SourceAndTargetSystem_JsonSchema> GetEnumerator()
        {
            return ((IEnumerable<SourceAndTargetSystem_JsonSchema>)_internallist).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_internallist).GetEnumerator();
        }
    }

    class SourceAndTargetSystem_JsonSchema
    {
            public string SystemType          {get; set;}
            public string JsonSchema         {get; set;}
            
    }
}


