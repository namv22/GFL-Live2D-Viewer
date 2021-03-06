using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using live2d;
using live2d.framework;

[ExecuteInEditMode]
public class SimpleModel : MonoBehaviour
{
    public TextAsset mocFile;
    public TextAsset physicsFile;
    public Texture2D[] textureFiles;
    public TextAsset motionFile;

    private Live2DMotion motion;
    private MotionQueueManager motionMgr;

    private Live2DModelUnity live2DModel;
    private EyeBlinkMotion eyeBlink = new EyeBlinkMotion();
    private L2DTargetPoint dragMgr = new L2DTargetPoint();
    private L2DPhysics physics;
    private Matrix4x4 live2DCanvasPos;




    void Start()
    {
        Live2D.init();
        load();

        motionMgr = new MotionQueueManager();
        motion = Live2DMotion.loadMotion(motionFile.bytes);       

       
    }

    void load()
    {
        live2DModel = Live2DModelUnity.loadModel(mocFile.bytes);

        for (int i = 0; i < textureFiles.Length; i++)
        {
            live2DModel.setTexture(i, textureFiles[i]);
        }

        float modelWidth = live2DModel.getCanvasWidth();
        live2DCanvasPos = Matrix4x4.Ortho(0, modelWidth, modelWidth, 0, -50.0f, 50.0f);

        if (physicsFile != null) physics = L2DPhysics.load(physicsFile.bytes);
    }


    void Update()
    {
        if (live2DModel == null) load();
        live2DModel.setMatrix(transform.localToWorldMatrix * live2DCanvasPos);
        if (!Application.isPlaying)
        {
            live2DModel.update();
            return;
        }

        var pos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            //
        }
        else if (Input.GetMouseButton(0))
        {
            dragMgr.Set(pos.x / Screen.width * 2 - 1, pos.y / Screen.height * 2 - 1);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragMgr.Set(0, 0);
        }
        
        live2DModel.update();

        if (motionMgr.isFinished())
        {
            motionMgr.startMotion(motion);
        }

        motionMgr.updateParam(live2DModel);

        live2DModel.update();

    }


    void OnRenderObject()
    {
        if (live2DModel == null) load();
        live2DModel.setMatrix(transform.localToWorldMatrix * live2DCanvasPos);

        if (!Application.isPlaying)
        {
            live2DModel.update();
            live2DModel.draw();
            return;
        }

        dragMgr.update();
        // Direction of the face
        live2DModel.setParamFloat("PARAM_ANGLE_X", dragMgr.getX() * 30);
        live2DModel.setParamFloat("PARAM_ANGLE_Y", dragMgr.getY() * 30);
        // Direction of the body
        live2DModel.setParamFloat("PARAM_BODY_ANGLE_X", dragMgr.getX() * 10);
        // Eye movement
        live2DModel.setParamFloat("PARAM_EYE_BALL_X", -dragMgr.getX());
        live2DModel.setParamFloat("PARAM_EYE_BALL_Y", -dragMgr.getY());
        // Breathing
        double timeSec = UtSystem.getUserTimeMSec() / 1000.0;
        double t = timeSec * 2 * Math.PI;
        live2DModel.setParamFloat("PARAM_BREATH", (float)(0.5f + 0.5f * Math.Sin(t / 3.0)));
        // Blink
        eyeBlink.setParam(live2DModel);

        if (physics != null) physics.updateParam(live2DModel);


        live2DModel.update();
        live2DModel.draw();
    }
}