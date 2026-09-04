
// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

public static class SubComponents
{
    extension(Element element)
    {
        public List<Element> GetAllSubComponents()
        {
            var result = new List<Element>();
            if (element is not FamilyInstance familyInstance) return result;
            var subComponentIds = familyInstance.GetSubComponentIds();
            foreach (var subId in subComponentIds)
            {
                var subElement = element.Document.GetElement(subId);
                if (subElement == null) continue;
                result.Add(subElement);
                result.AddRange(subElement.GetAllSubComponents());
            }
            return result;
        }
    }
}