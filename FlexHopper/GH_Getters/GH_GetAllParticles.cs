﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using FlexCLI;

using FlexHopper.Properties;

namespace FlexHopper.GH_Getters
{
    public class GH_GetAllParticles : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetAllParticles class.
        /// </summary>
        public GH_GetAllParticles()
          : base("Get All Particles", "AllParts",
              "Get all particles from engine object",
              "Flex", "Decomposition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n", "n", "You can chose to only draw particles every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance.", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Vector", "Vec", "", GH_ParamAccess.tree);
        }

        int n = 1;
        int counter = 0;

        GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
        GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData(1, ref n);
            n = Math.Max(1, n);

            if (counter % n == 0)
            {
                Flex flex = null;


                DA.GetData(0, ref flex);

                if (flex != null)
                {
                    List<FlexParticle> part = flex.Scene.GetAllParticles();

                    pts = new GH_Structure<GH_Point>();
                    vel = new GH_Structure<GH_Vector>();

                    foreach (FlexParticle fp in part)
                    {
                        GH_Path p = new GH_Path(fp.GroupIndex);
                        pts.Append(new GH_Point(new Point3d(fp.PositionX, fp.PositionY, fp.PositionZ)), p);
                        vel.Append(new GH_Vector(new Vector3d(fp.VelocityX, fp.VelocityY, fp.VelocityZ)), p);
                    }
                }
            }

            DA.SetDataTree(0, pts);
            DA.SetDataTree(1, vel);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getParticles;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5dac11e5-91fb-4ec3-adf8-5b2e06404e05}"); }
        }
    }
}