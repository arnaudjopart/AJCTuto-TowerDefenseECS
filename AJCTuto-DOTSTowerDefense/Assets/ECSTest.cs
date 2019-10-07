using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;


public class ECSTest : MonoBehaviour
{
    public Mesh m_base;

    public Mesh m_top;

    //public static NativeList<TowerObject> m_towerObjects;

    public Material m_baseMaterial;
    public Material m_topMaterial;
    private EntityManager m_manager;

    public Mesh m_cube;
    public Material m_cubeMaterial;

    // Start is called before the first frame update
    void Start()
    {
        float3 spawnPosition = new float3(5,0,5);
        
        m_manager = World.Active.EntityManager;
        
        Entity baseOfEntity = m_manager.CreateEntity(
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(TowerBase));
        
        
        m_manager.SetSharedComponentData(baseOfEntity, new RenderMesh
        {
            mesh = m_base,
            material = m_baseMaterial,
            castShadows = ShadowCastingMode.On
        });
        
        m_manager.SetComponentData(baseOfEntity, new Translation
        {
            Value = spawnPosition
        });
        
        m_manager.SetComponentData(baseOfEntity, new Rotation()
        {
            Value = Quaternion.identity
        });
        
        Entity topOfTower = m_manager.CreateEntity(typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(SubEntity),
            typeof(Rotation),
            typeof(RelativeRotation));
        
        m_manager.SetSharedComponentData(topOfTower, new RenderMesh
        {
            mesh = m_top,
            material = m_topMaterial,
            castShadows = ShadowCastingMode.On
        });
        
        m_manager.SetComponentData(topOfTower, new Translation
        {
            Value = spawnPosition + new float3(0,2.2f,-2)
        });
        
        m_manager.SetComponentData(topOfTower, new RelativeRotation
        {
            m_currentAngle = 0,
            m_rotationSpeed = -100
        });
        
        m_manager.SetComponentData(topOfTower, new Rotation()
        {
            Value = Quaternion.Euler(0,0,0)
        });
        
        m_manager.SetComponentData(topOfTower, new SubEntity()
        {
            m_base = baseOfEntity,
            m_relativePosition = new float3(0,2.2f,-2)
        });
        
        Entity tipOfCanon = m_manager.CreateEntity(typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(SubEntity),
            typeof(Rotation),
            typeof(RelativeRotation));
        
        m_manager.SetSharedComponentData(tipOfCanon, new RenderMesh
        {
            mesh = m_cube,
            material = m_cubeMaterial,
            
        });
        
        m_manager.SetComponentData(tipOfCanon, new Translation
        {
            Value = spawnPosition + new float3(0,2.15f,1)
        });
        
        m_manager.SetComponentData(tipOfCanon, new RelativeRotation
        {
            m_currentAngle = 0,
            m_rotationSpeed = -0
        });
        
        m_manager.SetComponentData(tipOfCanon, new Rotation()
        {
            Value = Quaternion.Euler(0,0,0)
        });
        
        m_manager.SetComponentData(tipOfCanon, new SubEntity()
        {
            m_base = topOfTower,
            m_relativePosition = new float3(0,-0.05f,3.2f)
        });

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateHoverTank()
    {
        
    }
    
   
}


public struct TowerBase : IComponentData
{
    
}

public struct RelativeRotation : IComponentData
{
    public Quaternion Value;
    public float m_currentAngle;
    public float m_rotationSpeed;
}

public struct SubEntity : IComponentData
{
    public Entity m_base;
    public float3 m_relativePosition;
}

public class MoveForwardSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll(typeof(TowerBase)).ForEach((Entity _entity, ref Translation _translation, ref Rotation 
        _rotation) =>
            {
                _translation.Value += Time.deltaTime * new float3(0, 0, 1);
                //_rotation.Value *= Quaternion.Euler(0, 10 * Time.deltaTime, 0);

            });
    }
}


public class LinkTopPositionToBaseSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll((typeof(SubEntity))).ForEach((Entity _entity, ref Translation _translation, ref 
        RelativeRotation _relativeRotation,ref Rotation _rotation, ref SubEntity _towerTop) =>
        {
            if (_towerTop.m_base != Entity.Null)
            {
                Translation baseTranslation = EntityManager.GetComponentData<Translation>(_towerTop.m_base);
                Rotation baseRotation = EntityManager.GetComponentData<Rotation>(_towerTop.m_base);

                _translation.Value = (Vector3) baseTranslation.Value +
                                     (Quaternion) baseRotation.Value * _towerTop.m_relativePosition;
            }
        });
    }
}
 
public class RelativeRotationFromBaseSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll((typeof(SubEntity))).ForEach((Entity _entity, ref RelativeRotation _relativeRotation, ref Rotation 
        _rotation, ref SubEntity _towerTop) =>
        {
            if (_towerTop.m_base != Entity.Null)
            {
                _relativeRotation.m_currentAngle += _relativeRotation.m_rotationSpeed * Time.deltaTime;
                Rotation baseRotation = EntityManager.GetComponentData<Rotation>(_towerTop.m_base);
                Quaternion relativeRotation = (baseRotation.Value * Quaternion.Euler(0, _relativeRotation.m_currentAngle, 0));
                _rotation.Value = relativeRotation;
            }
        });
    }
}


 








