﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using FlexHopper.Properties;
namespace FlexHopper.GH_Getters
{
    public class GH_InstantBake : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_InstantBake class.
        /// </summary>
        public GH_InstantBake()
          : base("Instant Bake", "Bake",
              "Bakes a objects and deletes them again when updated, to account for movement.",
              "Flex", "Decomposition")
        {
        }

        List<Guid> ids = new List<Guid> {};
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        int counter = 0;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddGeometryParameter("Objects", "Objects", "Objects to bake", GH_ParamAccess.list);
            pManager.AddTextParameter("Layer Name", "Layer", "", GH_ParamAccess.item, "Default");
            pManager.AddTextParameter("Material Name", "Mat", "Specify render material by its name in your Rhino material table.", GH_ParamAccess.item, "Default material");
            pManager.AddGenericParameter("Attributes (Optional)", "Att", "Add custom Rhino.DocObjects.ObjectAttributes - object. Either through scripting or by using Horster (or similar). If set this overrides the 'Layer Name' and 'Material Name' input", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Clear Layer", "ClearL", "<CAUTION!> This deletes all objects on the specified layer without further warning!", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[3].Optional = true;            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            List<IGH_GeometricGoo> objs = new List<IGH_GeometricGoo>();
            DA.GetDataList(0, objs);

            string layerName = "Default";
            string matName = "Default material";
            bool clearL = false;

            DA.GetData(1, ref layerName);
            DA.GetData(2, ref matName);            

            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
            att.LayerIndex = doc.Layers.Find(layerName, true);
            att.MaterialIndex = doc.Materials.Find(matName, true);

            DA.GetData(3, ref att);
            DA.GetData(4, ref clearL);

            //Delete objects by GUID
            doc.Objects.Delete(ids, true);
        

            for(int i = 0; i < objs.Count; i++)
            {
                GeometryBase gb;
                Point3d pt;
                if (ids.Count <= i) ids.Add(Guid.NewGuid());

                att.ObjectId = ids[i];
                if (objs[i].CastTo<GeometryBase>(out gb))
                    doc.Objects.Add(gb, att);

                else if (objs[i].CastTo<Point3d>(out pt))
                    doc.Objects.AddPoint(pt, att);

                else
                    throw new Exception("Object nr. " + i + " is not bakeable:\n" + objs[i].ToString());
            }

            if (clearL)
            {

                foreach (Rhino.DocObjects.RhinoObject o in doc.Objects.FindByLayer(layerName))
                    doc.Objects.Delete(o, true);
            }

            if(counter >= 10)
            {
                Rhino.RhinoDoc.ActiveDoc.ClearUndoRecords(true);
                counter = 0;
            }

            counter++;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.instant_bake;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e92927d9-cefe-4990-bc71-d25acf366180"); }
        }
    }
}