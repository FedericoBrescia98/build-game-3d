using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Person : MonoBehaviour
{
    public Transform PersonTransform;
    public Vector3 HomePosition = Vector3.zero;
    public Vector3 WorkPosition = Vector3.zero;
    public NavMeshAgent NavMeshAgent;
    public Animator Animator;
    public bool IsWalking = false;
    private TimeController _timeController;

    private void Awake()
    {
        GameObject timeController = GameObject.Find("TimeController");
        _timeController = timeController.gameObject.GetComponent<TimeController>();
        PersonTransform = this.transform;        
        NavMeshAgent.Warp(PersonTransform.position);
        StartCoroutine(Walk(HomePosition));
    }

    void Start()
    {
        Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Animator.SetFloat("vertical", IsWalking ? 1f : 0f);

        if(WorkPosition != Vector3.zero && HomePosition != Vector3.zero)
        {
            Vector3 persPos = PersonTransform.position;

            if (_timeController.CurrentTime.Hour > 18 || _timeController.CurrentTime.Hour < 6)
            {
                StartCoroutine(Walk(HomePosition));
                return;
            }

            if (persPos.x == WorkPosition.x && persPos.z == WorkPosition.z)
            {
                StartCoroutine(Walk(HomePosition));
            }
            else if (persPos.x == HomePosition.x && persPos.z == HomePosition.z)
            {
                StartCoroutine(Walk(WorkPosition));
            }
        }
        else
        {
            StartCoroutine(Walk(HomePosition));
        }
    }

    private IEnumerator Walk(Vector3 destination)
    {
        yield return new WaitForSeconds(2);
        IsWalking = true;
        NavMeshAgent.SetDestination(destination);
        while (PersonTransform.position.x != destination.x || PersonTransform.position.z != destination.z)
        {
            yield return null;
        }
        IsWalking = false;
    }

    [Serializable]
    private class SavePerson
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 homePosition;
        public Vector3 workPosition;
    }

    public string ToJson()
    {
        SavePerson savePerson = new SavePerson()
        {
            position = PersonTransform.position,
            rotation = PersonTransform.rotation,
            homePosition = HomePosition,
            workPosition = WorkPosition,
        };
        return JsonUtility.ToJson(savePerson);
    }

    public Person FromJson(string json)
    {
        SavePerson savePerson = JsonUtility.FromJson<SavePerson>(json);
        PersonTransform.position = savePerson.position;
        PersonTransform.rotation = savePerson.rotation;
        HomePosition = savePerson.homePosition;
        WorkPosition = savePerson.workPosition;
        return this;
    }
}
