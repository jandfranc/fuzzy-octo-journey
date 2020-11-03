using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 200f;
    [SerializeField] float mainThrust = 7f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngineSound = null;
    [SerializeField] AudioClip crashSound = null;
    [SerializeField] AudioClip levelCompleteSound = null;

    [SerializeField] ParticleSystem starboardParticles = null;
    [SerializeField] ParticleSystem portParticles = null;
    [SerializeField] ParticleSystem successParticles = null;
    [SerializeField] ParticleSystem crashParticles = null;

    Rigidbody rigidBody;
    AudioSource audioSource;
    enum State { Alive, Dying, Transcending };
    State state = State.Alive;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (state == State.Alive)
        {
            processThrusting();
            processTurning();
        }
        
    }

  
    private void processThrusting()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ActivateThrust();

        }
        else
        {
            DeactivateThrust();
        }
    }

    private void ActivateThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        starboardParticles.Play();
        portParticles.Play();
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngineSound);
        }
    }


    private void DeactivateThrust()
    {
        starboardParticles.Stop();
        portParticles.Stop();
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    

    private void processTurning()
    {

        if (!(Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)))
        {
            rigidBody.freezeRotation = true;
            float Torque = rcsThrust * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
            {
                
                transform.Rotate(Vector3.forward * Torque);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(-Vector3.forward * Torque);
            }
            rigidBody.freezeRotation = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                ProcessLvlComplete();
                break;
            default:
                ProcessCrash();
                break;

        }
    }

    private void ProcessLvlComplete()
    {
        if (state == State.Alive)
        {
            state = State.Transcending;
            audioSource.Stop();
            audioSource.PlayOneShot(levelCompleteSound);
            successParticles.Play();
            starboardParticles.Stop();
            portParticles.Stop();
            Invoke("LoadNextScene", levelLoadDelay);
        }
        
    }

    private void ProcessCrash()
    {
        if (state != State.Transcending)
        {
            state = State.Dying;
            rigidBody.constraints = RigidbodyConstraints.None;
            audioSource.Stop();
            audioSource.PlayOneShot(crashSound);
            crashParticles.Play();
            starboardParticles.Stop();
            portParticles.Stop();
            Invoke("LoadFirstScene", levelLoadDelay);
        }
        
    }

    private void LoadFirstScene()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }
}
