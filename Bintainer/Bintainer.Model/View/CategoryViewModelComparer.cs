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

            // Collect all IDs from the updated list including nested children
            var updatedIds = GetAllIds(updated);

            // Start the comparison
            CompareLists(original, updated, null, updatedIds);
        }

        private void CompareLists(List<CategoryViewModel> original, List<CategoryViewModel> updated, int? parentId, HashSet<int> updatedIds)
        {
            var originalDict = original.ToDictionary(item => item.Id ?? -1);

            // Process updated items
            foreach (var item in updated)
            {
                if (!originalDict.ContainsKey(item.Id ?? -1)) // New item
                {
                    item.ParentId = parentId;
                    Added.Add(item);
                }
                else // Existing item
                {
                    var originalItem = originalDict[item.Id ?? -1];
                    if (IsUpdated(originalItem, item))
                    {
                        Updated.Add(new CategoryViewModel { Title = item.Title, Id = item.Id });
                    }
                    else
                    {
                        Unchanged.Add(new CategoryViewModel { Title = originalItem.Title, Id = item.Id });
                    }

                    // Remove from dictionary to prevent marking as deleted later
                    originalDict.Remove(item.Id ?? -1);

                    // Recursively check children
                    if (item.Children != null && originalItem.Children != null)
                    {
                        CompareLists(originalItem.Children, item.Children, item.Id, updatedIds);
                    }
                }
            }

            // Process remaining items in the original list as deleted (only if they aren't present in the updated hierarchy)
            foreach (var remainingItem in originalDict.Values)
            {
                if (!updatedIds.Contains(remainingItem.Id ?? -1))
                {
                    Deleted.Add(remainingItem);
                }
            }
        }

        private bool IsUpdated(CategoryViewModel original, CategoryViewModel updated)
        {
            return original.Title != updated.Title;
        }

        private HashSet<int> GetAllIds(List<CategoryViewModel> categories)
        {
            var ids = new HashSet<int>();
            CollectIds(categories, ids);
            return ids;
        }

        private void CollectIds(List<CategoryViewModel> categories, HashSet<int> ids)
        {
            foreach (var category in categories)
            {
                if (category.Id.HasValue)
                    ids.Add(category.Id.Value);

                if (category.Children != null && category.Children.Count > 0)
                    CollectIds(category.Children, ids);
            }
        }
    }


}
