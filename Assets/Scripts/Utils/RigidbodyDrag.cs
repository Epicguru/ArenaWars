using UnityEngine;

public class RigidbodyDrag : MonoBehaviour
{
    public Camera Camera;
    [ReadOnly]
    public Rigidbody2D CurrentlySelected;
    public float Force = 5f;
    public AnimationCurve ForceCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public float MostForceDistance = 50f;

    [ReadOnly]
    public RaycastHit2D[] Hits = new RaycastHit2D[10];

    public void Update()
    {
        if (Camera == null)
            return;
        if (!Application.isEditor)
            return;

        if(CurrentlySelected == null && InputManager.IsDown("Select"))
        {
            int hits = Physics2D.GetRayIntersectionNonAlloc(MakeRay(), Hits);

            if(hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    RaycastHit2D hit = Hits[i];

                    Rigidbody2D rb = hit.rigidbody;
                    if(rb == null)
                    {
                        rb = hit.transform.GetComponentInParent<Rigidbody2D>();
                    }

                    if(rb != null)
                    {
                        if (CanDrag(rb))
                        {
                            CurrentlySelected = rb;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }
        
        if(CurrentlySelected != null)
        {
            if (!InputManager.IsPressed("Select"))
            {                
                CurrentlySelected = null;
            }
        }
    }

    public void FixedUpdate()
    {
        if(CurrentlySelected != null)
        {
            Vector2 force = InputManager.MousePos - (Vector2)CurrentlySelected.transform.position;
            force.Normalize();
            float dst = Vector2.Distance((Vector2)CurrentlySelected.transform.position, InputManager.MousePos);
            float strength = Mathf.Clamp(ForceCurve.Evaluate(1f - Mathf.Clamp(dst / MostForceDistance, 0f, 1f)), 0f, 1f);
            force *= (Force * strength);
            CurrentlySelected.velocity = force;
        }
    }

    public bool CanDrag(Rigidbody2D r)
    {
        return true;
    }

    public Ray MakeRay()
    {
        if (Camera == null)
            return new Ray();

        Ray r = Camera.ScreenPointToRay(InputManager.ScreenMousePos);

        return r;
    }
}