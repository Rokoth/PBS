using ProjectBranchSelector.Common;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProjectBarchSelector.DesktopApp.Service
{
    


    public class FileService : IFileService
    {

        public IEnumerable<TreeItemModel> GetTreeItemsFromFS(string path, Guid treeId)
        {
            if (Directory.Exists(path))
            {
                return GetTreeItemsFromFSInternal(path, treeId, null);
            }
            else throw new Exception($"Path {path} not exists");
        }

        private IEnumerable<TreeItemModel> GetTreeItemsFromFSInternal(string path, Guid treeId, Guid? parentId)
        {
            var id = Helper.GenerateGuid(new string[] { Path.GetDirectoryName(path) });
            yield return new TreeItemModel()
            {
                Id = id,
                IsLeaf = false,
                Name = Path.GetDirectoryName(path),
                Description = Path.GetDirectoryName(path),
                ParentId = parentId,
                TreeId = treeId
            };

            foreach (var dir in Directory.GetDirectories(path))
            {
                foreach (var res in GetTreeItemsFromFSInternal(dir, treeId, id))
                {
                    yield return res;
                }
            }

            foreach (var file in Directory.GetFiles(path))
            {
                yield return new TreeItemModel()
                {
                    Id = Helper.GenerateGuid(new string[] { id.ToString(), Path.GetFileNameWithoutExtension(file) }),
                    IsLeaf = true,
                    Name = Path.GetFileNameWithoutExtension(file),
                    Description = Path.GetFileNameWithoutExtension(file),
                    ParentId = id,
                    TreeId = treeId
                };
            }
        }
    }
}
