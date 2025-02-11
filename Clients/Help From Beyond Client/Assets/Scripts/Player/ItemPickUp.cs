using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private GhostValues _ghostValues;
    private WizardValues _wizardValues;
    private MyInputManager _inputs;

    [SerializeField] private GameObject _heldItem;
    private const float _heldItemOffset = 0.7f;
    [SerializeField] private GameObject _objectInrange;

    private void Start()
    {
        if (gameObject.CompareTag("Ghost"))
        {
            _ghostValues = GetComponent<GhostValues>();
        }

        else if (gameObject.CompareTag("Wizard"))
        {
            _wizardValues = GetComponent<WizardValues>();
        }
        _wizardValues = GetComponent<WizardValues>();
        _ghostValues = GetComponent<GhostValues>();
        _inputs = GetComponentInParent<MyInputManager>();
        Physics2D.IgnoreLayerCollision(11, 14);
        Physics2D.IgnoreLayerCollision(7, 14);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("Trigger");
        if (other.CompareTag("InstanceItem"))
        {
            Key key = other.GetComponentInParent<Key>();
            if (gameObject.CompareTag("Ghost"))
            {
                if (key)
                {
                    if (!key._inWizardReality) _objectInrange = other.transform.parent.gameObject;
                }
                
            }

            else if (gameObject.CompareTag("Wizard"))
            {
                if (key)
                {
                    if (key._inWizardReality) _objectInrange = other.transform.parent.gameObject;
                }
               
            }


        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        _objectInrange = null;
    }

    private void Update()
    {
        if (_inputs.GhostInteractPerformedThisFrame())
        {
            if (gameObject.tag == "Ghost")
            {
                if (_heldItem) ThrowItem();
                else if (_objectInrange)
                {
                    PickUpItem(_objectInrange);
                }
            }
            
        }

        if (_inputs.WizardInteractPerformedThisFrame())
        {
            print("grab");
            if (gameObject.tag == "Wizard")
            {
                if (_heldItem) ThrowItem();
                else if (_objectInrange)
                {
                    PickUpItem(_objectInrange);
                }
            }

        }
    }

    public void ThrowItem()
    {
        if (gameObject.tag == "Ghost")
        {
            gameObject.layer = 11;
        }
        else if (gameObject.tag == "Wizard")
        {
            gameObject.transform.Find("Sprite").gameObject.layer = 7;
        }

        _heldItem.GetComponent<Rigidbody2D>().isKinematic = false;
        _heldItem.transform.SetParent(null, true);

        if (gameObject.CompareTag("Wizard"))
        {
            Vector2 aim = _inputs.WizardAim();
            _heldItem.GetComponent<Rigidbody2D>().AddForce(aim * _wizardValues.throwForce, ForceMode2D.Impulse);
        }
        else
        {
            Vector2 aim = _inputs.GhostAim();
            _heldItem.GetComponent<Rigidbody2D>().AddForce(aim * _ghostValues.throwForce, ForceMode2D.Impulse);
        }
        
        _heldItem.GetComponentInChildren<CircleCollider2D>().enabled = true;
        _heldItem = null;
    }

    public void PickUpItem(GameObject item)
    {
        if (gameObject.tag == "Ghost")
        {
            gameObject.layer = 18;
        }
        else if (gameObject.tag == "Wizard")
        {
            gameObject.transform.Find("Sprite").gameObject.layer = 19;
        }

        _heldItem = item;
        item.GetComponent<Key>().Interact();
        item.transform.eulerAngles = Vector3.zero;
        _heldItem.transform.parent = transform;
        _heldItem.transform.localPosition = Vector3.up * _heldItemOffset;
        item.GetComponentInChildren<CircleCollider2D>().enabled = false;
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        
    }

    public void ReleaseKey()
    {
        if (gameObject.tag == "Ghost")
        {
            gameObject.layer = 11;
        }
        else if (gameObject.tag == "Wizard")
        {
            gameObject.transform.Find("Sprite").gameObject.layer = 7;
        }
        _heldItem.transform.SetParent(null, true);
        _heldItem = null;      
    }

    public void DropKey()
    {
        if (gameObject.tag == "Ghost")
        {
            gameObject.layer = 11;
        }
        else if (gameObject.tag == "Wizard")
        {
            gameObject.transform.Find("Sprite").gameObject.layer = 7;
        }
        if (_heldItem)
        {
            _heldItem.GetComponent<Rigidbody2D>().isKinematic = false;
            _heldItem.GetComponentInChildren<CircleCollider2D>().enabled = true;
            _heldItem.transform.SetParent(null, true);
            _heldItem = null;
        }
        
    }
}
