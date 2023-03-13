using CoreSystem;
using System.Collections.Generic;
using UnityEngine;

namespace RouteGames
{
    public class PauseManager : MonoSingleton<PauseManager>
    {
        private List<KeyValuePair<Animator, float>> m_pausedAnimators = new List<KeyValuePair<Animator, float>>();
        private List<AudioSource> m_pausedAudioSources = new List<AudioSource>();

        public bool IsMenuPaused
        {
            get
            {
                return m_IsMenuPaused;
            }
        }

        public bool IsSequencePause
        {
            get
            {
                return m_IsSequencePaused;
            }
        }

        private bool m_IsMenuPaused = false;
        private bool m_IsSequencePaused = false;
        private HashSet<IPausable> m_RegisteredPausables = new HashSet<IPausable>();

        private enum PauseRequestType
        {
            GAME_PAUSE = 0,         // 0000
            GAME_RESUME = 1,        // 0001
            MENU_PAUSE = 2,         // 0010
            MENU_RESUME = 3         // 0011 
            /*NEXT_PAUSE = 4,       // 0100
            NEXT_RESUME = 5,        // 0101
            NEXT_PAUSE2 = 8,        // 1000
            NEXT_RESUME2 = 9        // 1001*/
        }

        private bool HandlePauseRequest(PauseRequestType requestType, ref bool pauseState)
        {
            if (((int)requestType & 0x01) == 1)
            {
                if (!pauseState)
                {
                    return false;
                }

                pauseState = false;
            }
            else
            {
                if (pauseState)
                {
                    return false;
                }

                pauseState = true;
            }

            foreach (IPausable pausable in m_RegisteredPausables)
            {
                switch(requestType)
                {
                    case PauseRequestType.GAME_PAUSE:
                        {
                            pausable.MenuPause();
                            break;
                        }
                    case PauseRequestType.GAME_RESUME:
                        {
                            pausable.MenuResume();
                            break;
                        }
                    case PauseRequestType.MENU_PAUSE:
                        {
                            pausable.SequencePause();
                            break;
                        }
                    case PauseRequestType.MENU_RESUME:
                        {
                            pausable.SequenceResume();
                            break;
                        }
                }
            }

            return true;
        }

        public bool MenuPause()
        {
            PauseAllAnimators();
            PauseAllAudioSources();
            return HandlePauseRequest(PauseRequestType.GAME_PAUSE, ref m_IsMenuPaused);
        }

        public bool MenuResume()
        {
            ResumeAllAnimators();
            ResumeAllAudioSources();
            return HandlePauseRequest(PauseRequestType.GAME_RESUME, ref m_IsMenuPaused);
        }

        public bool SequencePause()
        {
            return HandlePauseRequest(PauseRequestType.MENU_PAUSE, ref m_IsSequencePaused);
        }

        public bool SequenceResume()
        {
            return HandlePauseRequest(PauseRequestType.MENU_RESUME, ref m_IsSequencePaused);
        }

        public bool Register(IPausable pausable)
        {
            return m_RegisteredPausables.Add(pausable);
        }

        public bool Unregister(IPausable pausable)
        {
            return m_RegisteredPausables.Remove(pausable);
        }


        private void PauseAllAnimators()
        {
            GameObject sequenceObjects = GameObject.Find("SequenceObjects");

            if (sequenceObjects)
            {
                foreach (Animator animator in sequenceObjects.GetComponentsInChildren<Animator>())
                {
                    m_pausedAnimators.Add(new KeyValuePair<Animator, float>(animator, animator.speed));
                    animator.speed = 0.0f;
                }
            }

            GameObject miniGames = GameObject.Find("MiniGames");

            if (miniGames)
            {
                foreach (Animator animator in miniGames.GetComponentsInChildren<Animator>())
                {
                    m_pausedAnimators.Add(new KeyValuePair<Animator, float>(animator, animator.speed));
                    animator.speed = 0.0f;
                }
            }
        }

        private void ResumeAllAnimators()
        {
            foreach (KeyValuePair<Animator, float> animator in m_pausedAnimators)
            {
                if (animator.Key != null)
                    animator.Key.speed = animator.Value;
            }

            m_pausedAnimators.Clear();
        }

        private void PauseAllAudioSources()
        {
            GameObject sequenceObjects = GameObject.Find("SequenceObjects");

            if (sequenceObjects)
            {
                foreach (AudioSource audioSources in sequenceObjects.GetComponentsInChildren<AudioSource>())
                {
                    m_pausedAudioSources.Add(audioSources);
                    audioSources.Pause();
                }
            }

            GameObject miniGames = GameObject.Find("MiniGames");

            if (miniGames)
            {
                foreach (AudioSource audioSources in miniGames.GetComponentsInChildren<AudioSource>())
                {
                    m_pausedAudioSources.Add(audioSources);
                    audioSources.Pause();
                }
            }

            AudioManager.Instance.PauseAudio(CoreSystem.AudioType.Dialogue);
            AudioManager.Instance.CustomAudioSource.Pause();
        }

        private void ResumeAllAudioSources()
        {
            foreach (AudioSource audioSources in m_pausedAudioSources)
            {
                if (audioSources != null)
                    audioSources.UnPause();
            }

            m_pausedAudioSources.Clear();

            AudioManager.Instance.ResumeAudio(CoreSystem.AudioType.Dialogue);
            AudioManager.Instance.CustomAudioSource.UnPause();
        }
    }
}