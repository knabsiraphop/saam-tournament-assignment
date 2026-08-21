# ส่วนที่ 3 — Code Review

## 1. Review ตามที่จะเขียนจริงใน Pull Request

- แก้ compile error ด้วยครับ
- แก้เรื่อง Determinism (random,update,deltatime)
- ปรับ Code style ไม่ตรงกับทีม (serializefield , encapsulate , ghost number)
- ปรับ การทำงานกระจุกตัวอยู่ในโค้ดเดียวเกินไป (srp)
- ใช้ unitask แทน ienumerator และ ใช้ $"" แทน ""+""
- แก้ spawn object ไม่ถูก destroy
- แก้ เกม infinity loop ไม่มีวันจบ
- Client ส่ง score ไปตรงๆ ไม่มี token และ ไม่มีการ verify replay record — อันนี้ต้องคุยกับทีม backend...
- เพิ่ม error handler ใน request
- singleton ไม่ได้เช็ค Instance Null? ทำให้มี Instance ใหม่โดยที่ instance เก่ายังอยู่
- เข้ารหัส player pref
- ไม่แน่ใจว่าทำไมเรียก CheckCollision เอง ทำไมไม่ใช้ observer กับ api ของ unity (oncollisionenter)

## 2. จัดกลุ่มประเด็น

ต้องแก้ก่อน Merge

1 Compile error
- ไม่ได้ใส่ using unityengine.ui(text) และ using system.collection(ienumerator)
- ไม่มี function SpawnObstacle กับ CheckCollision
- ปกติผมไม่ได้เช็ค compile error ตอนตรวจ pr อันนี้ผมเอาโค้ดมาลอง compile อาจจะ โกงไปหน่อย

2 Code style
- ทุก global variable เป็น public ทั้งหมด ไม่สามารถบอกได้ว่าตัวไหนต้องการจะเป็น serializefield หรือ ไม่เป็น
- ทุก global variable ไม่มีความจำเป็นต้องเป็น public เลย ยกเว้น Instance ส่วนตัวอื่นๆสามารถใช้ [serializefield] private แทนได้ ถ้าอยากให้ขึ้นใน Inspector
- Ghost number เยอะมากต้องประกาศเป็น private const ไม่ก็ แยกไฟล์ config.cs หรือ const.cs ไปเลย
- โค้ดนี้เป็นโค้ด gamemanager ควรเป็นหน่อยควบคุม feature ใหญ่อื่นๆ ผิดหลัก SRP ซึ่งโค้ดนี้สามารถกระจายไปยังระบบอื่นๆได้ดังนี้  ระบบคำนวนความเร็ว,ระบบคำนวนคะแนน,ระบบ spawn object,ระบบsound, ระบบ localsave , ระบบsendrequest
- ทุก function ต้องใส่ encapsulate private public

3. Perfomance
- ห้ามใช้ ienumerator  ใช้ unitask เท่านั้น ในโค้ดตอนนี้ไม่สามาถ cancel ได้ ไม่รู้เลยว่าทำเสร็จเมื่อไร
- ห้ามใช้ "text" + "text" การทำแบบนี้จะสร้าง gc แถมอยู่ใน update ด้วย ใช้ $"{text}" หรือ string.format หรือ Zstring

4 Risk
- ต้อง random จาก seed ให้ตรงกับ requirement
- การใช้ update กับ time.deltatime ช่วยแค่การแสดงผลให้ดูเสมือน เท่านั้นคะแนนและความเร็วที่คำนวนออกมาไม่ได้ Determinism
- มีแต่การ spawn obstacle ไม่มีการ destrop / release / return
- เมื่อ gameover ไม่มีการ return ออกจาก loop สามารถเกิดการีนก gameover ซ้ำๆ เกิดการยิงรีเควสซ้ำๆ แล้วเกมก็ไม่จบ update ยังทำต่อ เพราะไม่มี flag ตัวไหนบอกว่าเกมรันอยู่รึเปล่า
- ไม่มี error handler หรือ เช็ค status request ที่ได้มา
- Client ส่ง id และ score ไปตรงๆ ไม่มี token/signature และ ไม่มี replay record อันนี้ต้องคุยกับทีม backend แก้ server ให้รับ token/signature และ replay record ไปด้วย

ควรแก้แต่รอได้
1. Singleton ไม่มีการเช็คว่ามี instance เดิมอยู่เปล่า อาจทำให้มี object ค้างใน scene
2. key playerpref ควรเข้ารหัส ไม่ควรใช้ key text ตรงๆ

ข้อสังเกต
1. Unity มี oncollionenter ontriggerenter อยู่แล้วทำไมต้องมาเรียกเช็ค CheckCollision() เอง ใช้เป็น observer ไปเลยไม่ได้หรอ

## 3. ประเด็นร้ายแรงที่สุด + วิธีแก้

เนื่องจากเป็นเกมที่มีผู้เล่นจำนวนมากการที่เกมบัค หรือ performance ไม่ดี ยังแย่น้อยกว่าการมีการแฮคคะแนน หรือ ส่งคะแนนผิด ประเด็นที่ควรแก้ที่สุดคือ score submission ตามที่เห็นการส่ง request จากใน code แสดงให้เห็นว่าเซิฟเวอร์ไม่ได้รับ payload token/signature , replay record ทำให้ server ไม่สามารถตรวจสอบคะแนนได้ token/signature เป็นเพียงเกราะชั้นแรก แต่ replay record เป็นตัวที่ใช้เช็คจริงๆ ต้องไปคุยกับฝั่งคนเขียน server ให้เค้ารับ token/signature และ verify score โดยใช้ replay record ด้วย

## 4. ถ้าเป็น Prototype ต้องส่งภายในวันพรุ่งนี้ — Review จะเปลี่ยนไปยังไง

ทุกอย่างเหมือนเดิมยกเว้น
- ปรับ การทำงานกระจุกตัวอยู่ในโค้ดเดียวเกินไป (srp)
- code style ทั้งหมด

เนื่องจากผมมองว่าเวลาทั้งหมดนี้ควรทำเสร็จในเวลาไม่กี่ชั่วโมงยกเว้นการปรับโครงสร้างโค้ดซึ่งอาจจะใหญ่ไป ถ้ารีบมีโอกาสเกิดบัคเพิ่มมากกว่าเดิม หรือถ้าไม่ทันจริงๆ การไม่ปรับไปใช้ unitask ก็สามารถละไว้ก่อนได้

---

## AI-usage disclosure

ผมไล่หาบั๊ก/ปัญหาในโค้ดด้วยตัวเองก่อนทุกข้อ แล้วให้ Claude (AI assistant) ช่วยตรวจทานเพิ่มเติมเป็นรอบๆ ไป ตามรายละเอียดนี้:

**ข้อ 2 (จัดกลุ่มประเด็น)** — ผมเขียน draft แรกเองก่อน (compile error, code style, SRP, UniTask/GC) แล้ว AI ช่วยตรวจทานและชี้จุดที่ผมพลาดไปทั้งหมด 4 จุด แบ่งเป็น 2 รอบ:
- รอบแรก: (1) ช่องโหว่ client-authoritative score submission (ส่ง score ผ่าน GET ไม่มี token/signature) และ (2) ความเชื่อมโยงระหว่าง `Random.value` ที่ไม่ seed กับ requirement เรื่อง Determinism ในส่วนที่ 1
- รอบสอง: (3) ไม่มี state guard กัน `GameOver()` ถูกเรียกซ้ำ (ทำให้ยิง request ซ้ำและเกมไม่จบ) และ (4) `obstacles` list ไม่มีการลบ object ที่ถูก destroy แล้ว เสี่ยง `MissingReferenceException`

ผมนำทั้ง 4 จุดนี้ไปเขียนเพิ่มเข้า list เองทั้งหมด (คำอธิบาย/เหตุผลเป็นของผม)

**ข้อ 1 (PR-style comment)** — ผมเขียนตามสไตล์ที่ใช้จริงในงาน (แบบห้วนๆ เป็น bullet) แล้ว AI ช่วยเทียบกับ list ในข้อ 2 ชี้ว่าข้อ 1 ที่ผมเขียนไว้ตอนแรกขาด blocker ไป 2 จุด คือ compile error และเรื่อง token/signature — ผมเพิ่มเข้าไปเอง ส่วนสไตล์การเขียน (ห้วน ไม่มี greeting) ผมยืนยันว่าเป็นของผมเอง ไม่ได้ปรับตาม AI

**ข้อ 3 (ประเด็นร้ายแรงสุด)** — ผมเลือกประเด็นและเขียนคำอธิบาย fix แรก (เพิ่ม token/signature ฝั่ง backend) เองทั้งหมด แล้ว AI อธิบายเพิ่มเติม (ผมถามเอง) เรื่อง:
- ความแตกต่างระหว่าง auth token ตอน login กับ signature ต่อ request (คนละปัญหากัน — token ยืนยันตัวตน ไม่ได้ยืนยันว่าค่า score เป็นความจริง)
- ข้อจำกัดของ signature-based fix (client secret ถูก extract ได้ในทางทฤษฎี) และทางเลือกที่แน่นกว่า (server re-simulate จาก seed+replay แทนการเชื่อ client)

ผมนำแนวคิดนี้มาเขียนเป็นย่อหน้าเสริมในคำตอบเอง (ประโยคเป็นของผม รวมถึงส่วนที่เพิ่มเองเรื่อง risk การ simulate คลาดเคลื่อน/ต้องปรับจูน ซึ่ง AI ไม่ได้พูดถึง)

**ข้อ 4 (ถ้าต้องส่งพรุ่งนี้)** — ผมเขียนคำตอบเองว่าจะเลื่อน SRP refactor ออกไปก่อน พร้อมเหตุผล แล้ว AI เสนอเพิ่มว่า UniTask migration ก็น่าจะเข้าเกณฑ์เดียวกัน (effort/risk สูง) ควรพิจารณาเลื่อนได้เหมือนกัน — ผมรับมาแต่ปรับเป็นเงื่อนไข ("ถ้าไม่ทันจริงๆ") แทนที่จะเลื่อนแบบเด็ดขาด เพราะยังอยากทำถ้ามีเวลาพอ

ตลอดทั้ง 4 ข้อ **AI ไม่ได้เขียนคำตอบแทนผม** — บทบาทคือช่วยตรวจทานหา blind spot ที่ผมมองข้าม และอธิบาย concept ที่ผมถามเพิ่มเมื่อไม่แน่ใจ (เช่น determinism, token vs signature) เนื้อหา/เหตุผล/ประโยคทั้งหมดในคำตอบเป็นของผมเขียนเอง

**การแก้ไขรอบสอง (หลังเขียน Part 1 เสร็จ)** — ผมให้ Claude ขอ second opinion จาก Fable (AI อีกตัว) ตรวจ Part 1 กับ Part 3 พร้อมกัน เพื่อเช็คว่าสองส่วนขัดแย้งกันเองมั้ย Fable เจอ 3 จุดที่ผมแก้เองทั้งหมด:
- ข้อ 2: `Random.value` ไม่ seed เดิมจัดไว้ใน "ข้อสังเกต" (ระดับต่ำสุด) แต่ขัดกับ Part 1 ที่บอกว่า seed คือ requirement หลักของทั้งโหมด ทัวร์นาเมนต์จะพังถ้าไม่มี ผมย้ายขึ้นไป "ต้องแก้ก่อน Merge" เอง
- ข้อ 1, 2: โค้ดใช้ `Update()` + `Time.deltaTime` (variable timestep) ซึ่งเป็นปัจจัยทำลาย Determinism ตามที่เขียนไว้เองใน Part 1 แต่รีวิวเดิมไม่เคยพูดถึงเลย ผมเพิ่มเข้าไปทั้งในข้อ 1 (PR comment) และข้อ 2 (Risk)
- ข้อ 3: คำอธิบาย fix เดิมเสนอ token/signature กับ server-replay เป็นทางเลือกที่เทียบเท่ากัน ("หรือ อีกทางหนึ่ง") ทั้งที่ token ยืนยันแค่ตัวตน ไม่ได้ยืนยันว่าค่า score จริง ผมแก้ประโยคให้ชัดว่า token/signature เป็นแค่ "เกราะชั้นแรก" ส่วน replay record คือตัวที่เช็คคะแนนจริง
- ข้อ 4: เดิมเลื่อนแค่ SRP อย่างเดียวตอนโปรโตไทป์ต้องส่งพรุ่งนี้ Fable ชี้ว่า code style อื่นๆ (ghost number, encapsulate) ก็เป็น risk-to-correctness ต่ำเหมือนกัน น่าจะเลื่อนได้ด้วย ผมเพิ่ม "code style ทั้งหมด" เข้าไปในรายการที่เลื่อนได้เอง

เนื้อหา/ประโยคที่แก้ทั้งหมดเป็นของผมเขียนเอง Fable มีบทบาทแค่ชี้จุดขัดแย้งข้ามเอกสาร ไม่ได้เขียนคำตอบแทน
