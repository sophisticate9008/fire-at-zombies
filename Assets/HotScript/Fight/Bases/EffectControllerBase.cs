using System.Collections.Generic;
using UnityEngine;

namespace FightBases
{
    public class EffectControllerBase : MonoBehaviour, IEffectController
    {
        public string EffectName { get; set; }
        public bool IsPlaying { get; set; } = false;
        public GameObject Enemy { get; set; }
        public virtual float BaseHeight { get; set; } = 5;
        public virtual void Init()
        {

            transform.SetParent(Enemy.transform);
            transform.position = Enemy.transform.position + new Vector3(0, 0, -0.01f);
            ChangeScale();
        }
        public virtual void ChangeScale()
        {
            // Vector3 scaleNew = Enemy.transform.localScale * 4;
            // transform.localScale = scaleNew;
        }
        public virtual void Play()
        {
            // ClearSameEffect();
        }

        public virtual void Stop()
        {
        }
        private void ClearSameEffect()
        {
            List<Transform> toRemove = new();

            foreach (Transform controller in Enemy.transform)
            {
                var effectController = controller.GetComponent<IEffectController>();
                if (effectController != null && effectController.EffectName == EffectName)
                {
                    toRemove.Add(controller);
                }
            }
            if(toRemove.Count <= 1) {
                return;
            }
            for(int i = toRemove.Count - 2; i >= 0; i--) {
                ObjectPoolManager.Instance.ReturnToPool(EffectName + "Pool", toRemove[i].gameObject);
            }
            // 遍历完成后，再执行移除操作
        }
    }
}
