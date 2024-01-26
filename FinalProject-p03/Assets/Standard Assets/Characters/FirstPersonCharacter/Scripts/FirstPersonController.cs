/*
using System;

namespace ConsoleApplication2
{
    internal class Program
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("Enter Your Password ..");
            int password = int.Parse(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Green;
            

            int t1 = 0;
            for (int i = 1; i < 9; i++)
            {
              
                int t2 = 0;
                for (int j = 0; j < 9; j++)
                {
                    t1 += j;
                }

                Console.WriteLine(t2);
                System.Threading.Thread.Sleep(100);
            }

            Console.WriteLine("Click To Retry ..");
            Console.ReadLine();
            Main(new string[0]);

        }
    }
}*/


using System;
using System.Collections;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;

        [SerializeField]
        private AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from.

        [SerializeField] private AudioClip m_JumpSound; // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound; // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        [ReadOnly] public int currentBullets;
        public int maxBullets;
        public bool reloading;
        public float range;
        public Transform firePlace;
        public Animator animatorController;
        public GameObject fxFire;
        public bool isShotting;
        public AudioSource audioSource;
        public AudioClip ac;

        public Camera camera;
        private PhotonView view;


        public void Shoot()
        {
            if (isShotting == false && vertical < 0.1f && reloading == false)
            {
                print("P1");
                if (currentBullets > 0)
                {
                    print("P2");
                    currentBullets--;
                    isShotting = true;
                    audioSource.PlayOneShot(ac);
                    Vector3 fwd = transform.TransformDirection(Vector3.forward) * range;
                    // Debug.DrawRay(firePlace.position , fwd);
RaycastHit hit;

                    if (Physics.Raycast(firePlace.position, fwd, out hit, range))
                    {
                        print(hit.transform.name);

                        if (hit.transform.CompareTag("Enemy"))
                        {
                            Destroy(hit.transform.gameObject);
                        }
                    }

                    print("P3");
                    animatorController.SetTrigger("shotting");

                    Destroy(Instantiate(fxFire, firePlace.position, Quaternion.identity), 1);
                }
                else
                {
                    reloading = true;
                    StartCoroutine(ReloadingGun());
                }
            }
        }

        IEnumerator ReloadingGun()
        {
            animatorController.SetTrigger("reloading");
            currentBullets = maxBullets;
            yield return new WaitForSeconds(1.20f);
            reloading = false;
            isShotting = false;
        }

        private float timeRestIsShotting = 1;
        private float tempRestIsShotting;

        private void Update2()
        {
            tempRestIsShotting += Time.deltaTime;

            if (tempRestIsShotting > timeRestIsShotting)
            {
                isShotting = false;
                tempRestIsShotting = 0;
            }

            if (Input.GetMouseButton(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(1))
            {
                camera.fieldOfView = 30;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                camera.fieldOfView = 60;
            }
        }


        // Use this for initialization
        private void Start()
        {
            Application.runInBackground = true;
            view = GetComponent<PhotonView>();
            Init();


            if (view.IsMine)
            {
                currentBullets = maxBullets;
            }
            else
            {
                Destroy(camera.gameObject);
            }

            print("view owner : " + view.AmOwner + "\n mine? : " + view.IsMine);

            if (view.IsMine)
            {
                m_CharacterController = GetComponent<CharacterController>();
                m_Camera = Camera.main;
                m_OriginalCameraPosition = m_Camera.transform.localPosition;
                m_FovKick.Setup(m_Camera);
                m_HeadBob.Setup(m_Camera, m_StepInterval);
                m_StepCycle = 0f;
                m_NextStep = m_StepCycle / 2f;
                m_Jumping = false;
                m_AudioSource = GetComponent<AudioSource>();
                m_MouseLook.Init(transform, m_Camera.transform);
            }
        }


        // Update is called once per frame
        private void Update()
        {
            if (view.IsMine)
            {
                Update2();
                RotateView();
                // the jump state needs to read here to make sure it is not missed
                if (!m_Jump)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }

                if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
                {
                    StartCoroutine(m_JumpBob.DoBobCycle());
                    PlayLandingSound();
                    m_MoveDir.y = 0f;
                    m_Jumping = false;
                }

                if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
                {
                    m_MoveDir.y = 0f;
                }

                m_PreviouslyGrounded = m_CharacterController.isGrounded;
            }
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            if (view.IsMine)
            {
                float speed;
                GetInput(out speed);
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                    m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

                m_MoveDir.x = desiredMove.x * speed;
                m_MoveDir.z = desiredMove.z * speed;


                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_Jump = false;
                        m_Jumping = true;
                    }
                }
                else
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }

                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

                ProgressStepCycle(speed);
                UpdateCameraPosition(speed);

                m_MouseLook.UpdateCursorLock();
            }
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude +
                                (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                               Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }

            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            if (view.IsMine)
            {
                Vector3 newCameraPosition;
                if (!m_UseHeadBob)
                {
                    return;
                }

                if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
                {
                    m_Camera.transform.localPosition =
                        m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                            (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                    newCameraPosition = m_Camera.transform.localPosition;
                    newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
                }
                else
                {
                    newCameraPosition = m_Camera.transform.localPosition;
newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
                }

                m_Camera.transform.localPosition = newCameraPosition;
            }
        }

        private float vertical;

        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            vertical = CrossPlatformInputManager.GetAxis("Vertical");

            animatorController.SetFloat("walking", vertical);
            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
// On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (view.IsMine)
            {
                Rigidbody body = hit.collider.attachedRigidbody;
                //dont move the rigidbody if the character is on top of it
                if (m_CollisionFlags == CollisionFlags.Below)
                {
                    return;
                }

                if (body == null || body.isKinematic)
                {
                    return;
                }

                body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
            }
        }

        public Image fillImage;
        public int maxHealth;
        private int currentHealth;

        public void Init()
        {
            fillImage = GameObject.Find("FF").transform.GetChild(1).GetComponent<Image>();
            maxHealth = 100;


            currentHealth = maxHealth;
            SetImg();
        }

        public void SetImg()
        {
            fillImage.fillAmount = ((float) currentHealth / maxHealth);
        }

        private void OnCollisionEnter(Collision other)
        {
            print("1111111");
            if (other.transform.CompareTag("Enemy"))
            {
                print("1111111");
                currentHealth -= 5;
                SetImg();
            }
        }
    }
}