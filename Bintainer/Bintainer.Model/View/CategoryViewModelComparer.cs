using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class CategoryViewModelComparer
    {
        public List<CategoryViewModel> Added { get; private set; }
        public List<CategoryViewModel> Deleted { get; private set; }
        public List<CategoryViewModel> Unchanged { get; private set; }
        public List<CategoryViewModel> Updated { get; private set; }

        public CategoryViewModelComparer(List<CategoryViewModel> original, List<CategoryViewModel> updated)
        {
            Added = new List<CategoryViewModel>();
            Deleted = new List<CategoryViewModel>();
            Unchanged = new List<CategoryViewModel>();
            Updated = new List<CategoryViewModel>();

            CompareLists(original, updated, null);
        }

        private void CompareLists(List<CategoryViewModel> original, List<CategoryViewModel> updated, int? parentId)
        {
            var originalDict = original.ToDictionary(item => item.Id ?? -1);

            foreach (var item in updated)
            {

                if (!originalDict.ContainsKey(item.Id ?? -1))
                {
                    item.ParentId = parentId;
                    Added.Add(item);
                }
                else
                {
                    var originalItem = originalDict.ContainsKey(item.Id ?? -1) ? originalDict[item.Id ?? -1] : null;
                    if (IsUpdated(originalItem, item))
                    {
                        Updated.Add(new CategoryViewModel { Title = item.Title, Id = item.Id });
                    }
                    else
                    {
                        Unchanged.Add(new CategoryViewModel { Title = originalItem.Title, Id = item.Id });
                    }
                    originalDict.Remove(item.Id ?? -1);
                }

            }

            Deleted.AddRange(originalDict.Values);

            foreach (var originalItem in original)
            {
                var updatedChild = updated.FirstOrDefault(u => u.Id == originalItem.Id);
                if (updatedChild != null)
                {
                    CompareLists(originalItem.Children, updatedChild?.Children ?? new List<CategoryViewModel>(), originalItem.Id); // Pass parent ID
                }
            }
        }

        private bool IsUpdated(CategoryViewModel original, CategoryViewModel updated)
        {
            return original.Title != updated.Title;
        }
    }
}
