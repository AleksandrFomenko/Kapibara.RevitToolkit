// ReSharper disable once CheckNamespace
namespace Kapibara.Core;

public static class SolidEextensions
{
    extension(Element element)
    {
        IEnumerable<Solid> GetSolids()
        {
            if (element == null) yield break;

            var options = new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            };
            var geometryElement = element.get_Geometry(options);
            if (geometryElement == null)
                yield break;

            foreach (var solid in ExtractSolidsFromGeometry(geometryElement))
            {
                yield return solid;
            }
        }

        public Solid? GetSolid()
        {
            var options = new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            };
            var geometryElement = element.get_Geometry(options);
            return geometryElement == null ? null : ExtractFirstSolidFromGeometry(geometryElement);
        }
    }
    
    private static Solid? ProcessFirstGeometryObject(GeometryObject? geomObj)
    {
        switch (geomObj)
        {
            case Solid { Volume: > 0 } solid:
                return solid;
            case GeometryInstance geomInstance:
            {
                var instanceGeometry = geomInstance.GetInstanceGeometry();
                if (instanceGeometry != null)
                {
                    return ExtractFirstSolidFromGeometry(instanceGeometry);
                }

                break;
            }
        }

        return null;
    }
    private static Solid? ExtractFirstSolidFromGeometry(GeometryElement geometryElement)
    {
        return geometryElement.Select(ProcessFirstGeometryObject).OfType<Solid>().FirstOrDefault();
    }
    
    private static IEnumerable<Solid> ExtractSolidsFromGeometry(GeometryElement geometryElement)
    {
        return geometryElement.SelectMany(ProcessGeometryObject);
    }
    
    private static IEnumerable<Solid> ProcessGeometryObject(GeometryObject geomObj)
    {
        switch (geomObj)
        {
            case Solid { Volume: > 0 } solid:
                yield return solid;
                break;
            case GeometryInstance geomInstance:
            {
                var instanceGeometry = geomInstance.GetInstanceGeometry();
                if (instanceGeometry != null)
                {
                    foreach (var instSolid in ExtractSolidsFromGeometry(instanceGeometry))
                    {
                        yield return instSolid;
                    }
                }

                break;
            }
        }
    }
}