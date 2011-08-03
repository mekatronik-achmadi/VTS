﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Controllers
{
    public class pMCDatabaseWriterController
    {
        IList<PhotonDatabaseWriter> _photonDatabaseWriters;
        IList<CollisionInfoDatabaseWriter> _collisionInfoDatabaseWriters;

        public pMCDatabaseWriterController(
            IList<PhotonDatabaseWriter> photonDatabaseWriters,
            IList<CollisionInfoDatabaseWriter> collisionInfoDatabaseWriters)
        {
            _photonDatabaseWriters = photonDatabaseWriters;
            _collisionInfoDatabaseWriters = collisionInfoDatabaseWriters;
        }

        public IList<PhotonDatabaseWriter> PhotonDatabaseWriters { get { return _photonDatabaseWriters; } set { _photonDatabaseWriters = value; } }
        public IList<CollisionInfoDatabaseWriter> CollisionInfoDatabaseWriters { get { return _collisionInfoDatabaseWriters; } set { _collisionInfoDatabaseWriters = value; } }

        /// <summary>
        /// Method to write to all surface VB databases
        /// </summary>
        /// <param name="photonDatabaseWriters"></param>
        /// <param name="dp"></param>
        public void WriteToSurfaceVirtualBoundaryDatabases(PhotonDataPoint dp, CollisionInfo collisionInfo)
        {
            foreach (var writer in _photonDatabaseWriters)
            {
                if (DPBelongsToSurfaceVirtualBoundary(dp, writer))
                {
                    writer.Write(dp);
                }
            }; 
            // not best design but may work for now
            foreach (var writer in _collisionInfoDatabaseWriters)
            {
                writer.Write(collisionInfo);
            };
        }

        /// <summary>
        /// Method to determine if photon datapoint should be tallied or not
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="photonDatabaseWriter"></param>
        /// <returns></returns>
        public bool DPBelongsToSurfaceVirtualBoundary(PhotonDataPoint dp,
            PhotonDatabaseWriter photonDatabaseWriter)
        {
            if ((dp.StateFlag.Has(PhotonStateType.PseudoDiffuseReflectanceVirtualBoundary) && // pMC uses regular PST
                 photonDatabaseWriter.VirtualBoundaryType == VirtualBoundaryType.pMCDiffuseReflectance))
            {
                return true;
            }
            return false;
        }
        public void Dispose()
        {
            foreach (var writer in _photonDatabaseWriters)
            {
                writer.Dispose();
            }
            foreach (var writer in _collisionInfoDatabaseWriters)
            {
                writer.Dispose();
            }
        }
      
    }
} 