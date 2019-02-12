
import unicodedata
import arcpy
from arcpy import env
import exceptions, sys, traceback
import os
import glob

try:
    # Set environment settings
    Messdatum = "20190201"
    Basis = "P:/LZ-Voest/Schrottplatz/2019"
    geoDB = "20190201_Schrottplatz.gdb"

    TINRef = r"P:/LZ-Voest/Schrottplatz/System/Urgelände/TIN/Urgelände Schrottplatz"

    #---------------------------------------
    GIS = Basis + "/" + Messdatum + "/GIS"
    env.workspace = GIS + "/FC/TIN"
    folder_Pointcloud = GIS + "/PC/*.txt"
    #single_Pointcloud = GIS + "/FC/PC/Pile_1.txt"
    tempSHP = "d:/GisData/temp.shp"
    geoDB = GIS + "/FC/" + geoDB
    FDSName = "Piles"

    print("       GIS: " + GIS)
    print("workspache: " + env.workspace)
    print("Pointcloud: " + folder_Pointcloud)
    print("     geoDB: " +geoDB)
    # Define the spatial reference using the srid
    sr = arcpy.SpatialReference(31255, 5195)
        # 31255 M31
        # 5195 Triest

    arcpy.CheckOutExtension("3D")
    arcpy.env.overwriteOutput=True

    list_of_PC_Files = glob.glob(folder_Pointcloud)

        #Create the elevation points
    for inFile in list_of_PC_Files:

        fileNameFull = os.path.splitext(inFile)[0]
        fileName = os.path.basename(fileNameFull)
        print(fileName)
        arcpy.ASCII3DToFeatureClass_3d(inFile, "XYZ", tempSHP,
                                       "MULTIPOINT", z_factor=1.0, 
                                       input_coordinate_system=sr, 
                                       average_point_spacing=0.25)
        
        #delineate
        arcpy.CreateTin_3d(fileName, sr, tempSHP)
        arcpy.DelineateTinDataArea_3d(fileName, 1)

        #Tin domain
        border = os.path.join(geoDB, FDSName, fileName + "_B")
        arcpy.TinDomain_3d(fileName, border, "POLYGON")

        #Volume
        outVol = os.path.join(geoDB, FDSName, fileName + "_V")
        arcpy.SurfaceDifference_3d(fileName, TINRef, outVol)
        
    print("...done")

   # except arcpy.ExecuteError:
   # print arcpy.GetMessages()
except:
    # Get the traceback object
    tb = sys.exc_info()[2]
    tbinfo = traceback.format_tb(tb)[0]
    # Concatenate error information into message string
    pymsg = 'PYTHON ERRORS:\nTraceback info:\n{0}\nError Info:\n{1}'\
          .format(tbinfo, str(sys.exc_info()[1]))
    msgs = 'ArcPy ERRORS:\n {0}\n'.format(arcpy.GetMessages(2))
    # Return python error messages for script tool or Python Window
    arcpy.AddError(pymsg)
    arcpy.AddError(msgs)