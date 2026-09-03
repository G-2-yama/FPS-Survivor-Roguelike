using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : PoolableObject
{
    [SerializeField] private TextMeshPro damageText;

    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float moveSpeed = 1.5f;

    private Coroutine popupCoroutine;

    public void Setup(
        int damage,
        Vector3 position,
        Transform player)
    {
        transform.position = position;

        damageText.text = damage.ToString();

        // Player�̕���������
        Vector3 direction =
            player.position - transform.position;

        // �㉺�����ɌX�������Ȃ��ꍇ
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(-direction);
        }

        popupCoroutine = StartCoroutine(PopupRoutine());
    }

    private IEnumerator PopupRoutine()
    {
        float timer = 0f;

        Color color = damageText.color;
        color.a = 1f;
        damageText.color = color;

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;

            // ��ɕ���
            transform.position +=
                Vector3.up * moveSpeed * Time.deltaTime;

            // ���X�ɓ�����
            color.a = 1f - timer / lifeTime;
            damageText.color = color;

            yield return null;
        }

        Release();
    }

    public override void OnGet()
    {
        Color color = damageText.color;
        color.a = 1f;
        damageText.color = color;
    }

    public override void OnRelease()
    {
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }
    }
}