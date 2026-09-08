# קובץ דפוסים — פיתוח XR שיקומי ב-Unity

מסמך אישי. אחרי כל בלוק פיצ'ר מתווספת כאן רשומה: איזו בעיה נפתרה, מה הדפוס, שלד הקוד, ומה המלכודת.
המטרה: לבנות פרויקט דומה בעתיד בלי לשאול אף אחד.

**מוסכמות:** הודעות `Debug.Log` באנגלית · הערות בקוד בעברית · שם הקובץ חייב להיות זהה לשם ה-class · שמות class באות גדולה.

---

## 1. הקמת סביבת VR שנבדקת בלי קסדה

**הבעיה:** לפתח ולבדוק חוויית VR כשאין מכשיר.

**הדפוס:** Unity 6 + XR Interaction Toolkit, ובדיקה בסימולטור המובנה.

**השלבים:**
1. פרויקט Universal 3D, ואז `Window → Package Manager → Unity Registry → XR Interaction Toolkit → Install`.
2. בלשונית **Samples** לייבא את **Starter Assets**.
3. `Edit → Project Settings → XR Interaction Toolkit` → לסמן **Use XR Interaction Simulator in Scenes**.
4. לגרור את ה-prefab **XR Origin (VR)** מ-Starter Assets לסצנה, ולמחוק את ה-**Main Camera** העצמאית (לריג יש מצלמה משלו תחת Camera Offset).
5. להוסיף **Plane** כרצפה.

**שליטה בסימולטור:** `Tab` מחליף בין ראש לשלט · `[` ו-`]` בוחרים שלט · `WASD` תזוזה · לחצן ימני בעכבר להסתכלות.

**המלכודות:**
- מחיקת המצלמה **שבתוך** הריג במקום החיצונית — הריג נשאר בלי עיניים.
- בלי רצפה עם collider, הריג נופל דרך העולם מיד עם Play (יש לו כוח משיכה).
- אין prefab של סימולטור בגרסה 3.x — רק הצ'קבוקס בהגדרות.
- זיהוי ידיים אמיתי דורש חבילה נפרדת (**XR Hands**). לתרגיל שמבוסס הושטת יד, **השלט הוא היד**.

---

## 2. זיהוי מגע בין היד למטרה

**הבעיה:** לדעת מתי היד של המשתמש נגעה באובייקט.

**הדפוס:** אזור טריגר על המטרה + תג על היד.

**מה צריך:**
- על **המטרה**: Collider עם `Is Trigger` מסומן, ו-**Rigidbody** עם `Is Kinematic` מסומן.
- על **השלט**: Collider קטן (Sphere Collider, רדיוס ~0.05) + תג ייעודי, למשל `Controller`.

```csharp
private void OnTriggerEnter(Collider other)
{
    if (!other.CompareTag("Controller")) return;   // מגיבים רק ליד
    // ...
}
```

**המלכודות:**
- `Is Trigger` לבד לא מספיק — **חייב Rigidbody על לפחות אחד** משני האובייקטים, אחרת המנוע כלל לא בודק התנגשות.
- בלי `Is Kinematic` המטרה תיפול מכוח המשיכה.
- הסקריפט חייב לשבת על האובייקט שיש לו את ה-Collider.

---

## 3. מיקום יחסי לגוף (הבסיס למדידת טווח תנועה)

**הבעיה:** מטרה בקואורדינטות קבועות היא סתם קובייה במרחב. מטרה שיקומית חייבת להיות ביחס ל**גוף המטופל**.

**הדפוס:** `נקודת ייחוס + כיוון × מרחק`. נקודת הייחוס = המצלמה (הראש), כמייצגת את הגוף.

```csharp
[SerializeField] private Transform bodyReference;   // גוררים לכאן את Main Camera

float angle = Random.Range(-50f, 50f);                          // קשת מול השחקן
Vector3 direction = Quaternion.Euler(0f, angle, 0f) * bodyReference.forward;
direction.y = 0f;                                               // משטיחים - הגובה נקבע בנפרד
direction = direction.normalized;

transform.position = bodyReference.position + direction * distance + Vector3.up * height;
```

**למה זה חשוב:** אותו חישוב הופך למדד הקליני — מרחק ההישג נמדד מהגוף:
```csharp
float reachDistance = Vector3.Distance(bodyReference.position, transform.position);
```
טווח תנועה = **כמה רחוק מהגוף הגעת**, לא כמה מרחק היד עברה.

**מושגים:** `Quaternion` = איך Unity שומר סיבובים · `Quaternion.Euler(0,y,0)` = סיבוב סביב הציר האנכי · כפל סיבוב בכיוון מסובב את הכיוון · `.normalized` = מקצר לאורך 1 כדי שהמרחק ייקבע רק על ידי המכפיל.

---

## 4. הסלמה הדרגתית של קושי

**הבעיה:** אימון טווח תנועה עובד רק אם דורשים קצת יותר בכל סבב.

**הדפוס:** מונה סבבים + צעד קבוע.

```csharp
float distance = startDistance + (distanceStep * round);
float height   = heightStep * round;
```

**עיקרון:** כל פרמטר קושי נחשף כ-`[SerializeField]` כדי שאפשר יהיה לכוונן מהעורך בלי לגעת בקוד — זה מה שיאפשר למטפל להתאים למטופל.

---

## 5. השהיה בלי להקפיא את המשחק (Coroutine)

**הבעיה:** להבהב בצבע לרבע שנייה. פונקציה רגילה שמחכה מקפיאה את כל המשחק.

**הדפוס:** Coroutine — פונקציה שיכולה לעצור באמצע, לתת למשחק להמשיך, ולחזור לאותה נקודה.

```csharp
using System.Collections;   // דרוש ל-IEnumerator

StartCoroutine(FlashThenMove());     // Unity מפעיל אותה

private IEnumerator FlashThenMove()
{
    targetRenderer.material.color = hitColor;
    yield return new WaitForSeconds(0.25f);   // עוצר כאן, המשחק ממשיך
    MoveToNextPosition();
}
```

**שלושת החלקים:** `IEnumerator` (C#) = סוג ההחזרה שמסמן "אפשר לעצור באמצע" · `yield return X` = נקודת העצירה, ו-X אומר למה לחכות · `StartCoroutine` (Unity) = מה שמפעיל בפועל.

**`WaitForSeconds` הוא סוג, לא פונקציה** — יוצרים אותו עם `new`.

**איפה עוד:** רצף דיבור של הדמות המנחה, הודעות מושהות, אנימציות פשוטות.

---

## 6. מניעת הפעלה כפולה (דגל busy)

**הבעיה:** יד שנשארת בתוך המטרה, או ריצוד פיזיקה, סופרים את אותה נגיעה כמה פעמים.

**הדפוס:** דגל בוליאני שנועל בזמן הטיפול.

```csharp
private bool isBusy = false;

if (isBusy || !other.CompareTag("Controller")) return;
// ...
isBusy = true;
// ... פעולה ...
isBusy = false;
```

**כלל אצבע:** בכל טיפול באירוע לשאול — *מה קורה אם זה נקרא פעמיים ברצף?*

---

## 7. הכרזה ומאזינים (אירוע סטטי)

**הבעיה:** בנגיעה אחת צריכים להגיב הניקוד, המדידה, הדמות המנחה ומסך הסיכום. אם המטרה תקרא לכולם ישירות — היא תצטרך להכיר את כולם, וכל תוספת תדרוש לשנות אותה.

**הדפוס:** המטרה **מכריזה**. מי שרוצה — **מאזין**.

**צד המשדר:**
```csharp
public static event System.Action<int, float> OnTargetReached;   // רשימת מנויים, מתחילה ריקה

OnTargetReached?.Invoke(round, reachDistance);   // מפעיל את כל מי שרשום
```

**צד המאזין:**
```csharp
private void OnEnable()  { ReachTarget.OnTargetReached += HandleTargetReached; }
private void OnDisable() { ReachTarget.OnTargetReached -= HandleTargetReached; }

private void HandleTargetReached(int roundNumber, float distance) { /* ... */ }
```

**פירוק:** `System.Action<int, float>` = מותר להירשם רק לפונקציות שמקבלות int ו-float · `+=` מוסיף לרשימה, `-=` מוציא · `.Invoke(...)` = "תכריז עכשיו" · **`?` = רק אם יש בכלל מאזינים** (בלעדיו התוכנית קורסת כשהרשימה ריקה).

**דימוי:** תחנת רדיו. המשדר משדר גם אם אף אחד לא מאזין, ולא אכפת לו מי כן.

**המלכודת:** לשכוח את ה-`-=` ← "מאזיני רפאים" שממשיכים להגיב אחרי שהאובייקט נעלם, וגורמים לספירה כפולה ולשגיאות.

---

## 8. ניהול מפגש וסיכום

**הבעיה:** תרגיל שיקומי צריך התחלה, סוף, ודוח.

**הדפוס:** מנהל שמאזין להכרזות, צובר נתונים, ומסיים כשהגיעו ליעד.

```csharp
private int completedTargets = 0;
private float bestReach = 0f;
private float sessionStartTime;
private List<float> allReachDistances = new List<float>();

private void Start() { sessionStartTime = Time.time; }

private void HandleTargetReached(int round, float reachDistance)
{
    completedTargets++;
    allReachDistances.Add(reachDistance);
    bestReach = Mathf.Max(bestReach, reachDistance);      // שמירת שיא

    if (completedTargets >= targetsPerSession) EndSession();
}

private void EndSession()
{
    float duration = Time.time - sessionStartTime;        // הפרש זמנים = משך
    target.gameObject.SetActive(false);                   // מכבה את המטרה
    // ... הדפסת סיכום ...
}
```

**מושגים:** `Time.time` = שניות מתחילת הריצה, הפרש בין שתי מדידות נותן משך · `Mathf.Max` = הגדול מבין השניים · `List<float>` עם `.Add()` ו-`.Count` · `SetActive(false)` מכבה אובייקט · `foreach` עובר על רשימה.

---

## עקרונות רוחביים

- **קודם זרימה מקצה לקצה, ליטוש רק אחר כך.** התחלה → תרגיל → סיכום, ואז יופי.
- **כל פרמטר קליני נחשף כ-`[SerializeField]`** — מרחקים, צעדים, מספר חזרות, זמנים. זה מה שהופך תרגיל ל"מותאם למטופל".
- **הפרדה בין מי שיוצר אירוע למי שמגיב לו** — דרך הכרזות. מאפשר להוסיף מדידה, דמות מנחה וממשק בלי לגעת בלוגיקת התרגיל.
- **`GetComponent<T>()` בלי ציון אובייקט מחזיר רכיב מאותו אובייקט שהסקריפט יושב עליו.** אין צורך "לנווט" אליו.
- **איך לזהות מה מובנה:** שם באות גדולה שלא כתבת = חלק בניין של Unity או C# (`Renderer`, `Vector3`, `Quaternion`, `Color`, `WaitForSeconds`, `IEnumerator`). שם באות קטנה = משתנה שאנחנו נתנו לו שם.

---

## מה עוד נוסיף

- [ ] ממשק בעולם התלת-ממדי (Canvas במצב World Space) — למה זה שונה מ-VR ל-דו-ממד
- [ ] שמירת נתונים לקובץ CSV
- [ ] דמות מנחה: מצבי אנימציה + רצף הנחיות + קול וכתוביות
- [ ] שכבת חיישן מדומה (ממשק + מימוש מזויף) להחלפה במכשיר האמיתי
