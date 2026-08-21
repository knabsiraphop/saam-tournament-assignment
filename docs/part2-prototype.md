# ส่วนที่ 2 — Prototype

เลือก **ตัวเลือก A — Deterministic Core + Replay Verification**

## ทำไมเลือก A

- อะไรคือความเสี่ยงที่สุดใน Design (เทียบกับตัวเลือกอื่น) ทำไมถึงต้องพิสูจน์ข้อนี้ก่อน : เนื่องจากส่วนนี้เป็น part หลักและเป็นจุดเริ่มต้นของทุกส่วนถ้าพลาดตั้งแต่ตรงนี้ถึงส่วนอื่นจะทำได้ดีแค่ไหนก็ไม่มีประโยชน์

## 1. แยก Simulation ออกจาก MonoBehaviour ให้รันได้โดยไม่ต้องพึ่ง Unity Player

- สถาปัตยกรรมที่ใช้ (sim class, accumulator, driver ฝั่ง client/mock-server) : ใช้ menu item run plain c# โดยทั้งหมดไม่ได้ผูกกับ monobehavior โดย menu item จะเป็นคน spawn ตัว simulator และ ทำการ start update และ dispose ตาม parameter ที่ได้ไป โดยแบ่งออกเป็น 2 mode record , replay และ ตัวคะแนนและ speed จะถูกคำนวนผ่าน updater ซึ่งอยู่ภายในตัว simulator
- การตีความ "รันได้โดยไม่ต้องพึ่ง Unity Player" ที่เลือกใช้ + เหตุผล : simulation สามารถใช้ได้โดยที่ไม่ต้องกดรัน แต่ยังมีจุดที่ใช้ unity engine เช่น Debug Log , JsonUtility , ReplayFileIO ซึ่ง ส่วนนี้สามารถปรับเปลี่ยนได้ตาม environment ที่ใช้รัน หลักๆต้องการโชว์ logic

## 2. Replay เก็บเฉพาะ Seed และลำดับ Input เท่านั้น

- Seed เป็นค่าที่ fixed ไว้สำหรับการ simulate ในสถานการณ์จริงค่านี้ต้องได้จาก server
- Input เก็บเป็น List เพื่อให้สะดวกต่อการเพิ่ม record

## 3. พิสูจน์ว่า Replay เดิมให้ผลลัพธ์ตรงกันทุกครั้ง แม้ Frame Rate ต่างกัน

- วิธีทดสอบ: รอบแรกเลือก menu Simulator/Record เพื่อเก็บ Replay ไว้ แล้วก็ลอง Replay ตามลำดับ 30 60 120

ผลลัพธ์จริง (record 1 ครั้งที่ 60fps แล้ว replay ตัวเดียวกันที่ 30/60/120fps):

| รอบ | ReplayTime (ms) | Score | Speed | Obstacles (checksum) |
|---|---|---|---|---|
| Record (60fps) | 0.0810 | 175 | 51.1 | 274 |
| Replay 30fps | 0.3062 | 175 | 51.1 | 274 |
| Replay 60fps | 0.0576 | 175 | 51.1 | 274 |
| Replay 120fps | 0.0353 | 175 | 51.1 | 274 |

- สรุปผล: จากการทดสอบทั้ง 4 รอบเห็นได้ว่าผลลัพธ์มีค่าเท่ากัน ในทุก framerate

## 4. ระบบตรวจสอบฝั่ง Server จำลอง

- รับ Replay แล้วยืนยันคะแนนได้ยังไง: รัน Record 1 รอบ และส่ง score และ replay ไปเข้า simulation อีกครั้งเพื่อให้ได้คะแนนใหม่ออกมา เปรียบเทียบกัน

ผลลัพธ์จริง (Client บันทึก+อ้าง score เอง → ส่ง replay ไป mock server → server re-simulate เอง เทียบกับ score ที่ client อ้าง):

| ฝั่ง | ReplayTime (ms) | Score | Speed | Obstacles (checksum) |
|---|---|---|---|---|
| Client (Record) | 0.0560 | 176 | 51.1 | 274 |
| Server (Verify) | 0.0463 | 176 | 51.1 | 274 |

**Submit Score : 176, Verify Score : 176** — ตรงกัน server เชื่อคะแนนนี้ได้

- เวลาที่ใช้ตรวจต่อ 1 Replay: ~0.048 ms (หมายเหตุ: วัดจาก Editor ยังไม่มี network/parse overhead จริงของ production)

---

## AI-usage disclosure

Core logic ทั้งหมด (`RunnerSimulation`, accumulator, `Replay`, `RunnerSimulationWindow`) ผมเขียนเอง AI มีบทบาท 2 อย่าง:

**อธิบาย concept ที่ถาม** — fixed tick คืออะไรและทำไมใช้ 1/60, accumulator ทำไมต้องเก็บเศษที่เหลือไว้ (carry-over) ไม่ทิ้ง, spiral of death คืออะไรและวิธีป้องกัน (cap จำนวน tick catch-up ต่อเฟรม), ทำไม `Tick()` ต้องไม่เป็น async, ความต่างระหว่าง record mode (input sample ต่อ outer call ซ้ำได้ในหลาย catch-up tick) กับ replay mode (input ต้อง index ตาม tick ไม่ใช่ตาม frame) — ทุกจุดผมตามด้วยการ trace/เขียนโค้ดเองแล้วเอา AI ช่วยเช็คว่าถูกมั้ย

**Review หา bug/gap** — AI ชี้ 3 รอบว่า outer loop ผูก tick count เข้ากับ `simulationRate` แทนที่จะผูกกับ `fps` (ทำให้เวลารวมที่จำลองผิด), `deltaTime` ที่ส่งเข้า constructor คำนวณจาก `fps` แทนที่จะเป็นค่าคงที่จริง, และ `RunnerSimulation` ไม่มีทาง "เล่นซ้ำ" จาก Replay ที่มีอยู่แล้ว (มีแต่โหมดบันทึกใหม่) ผมแก้เองทุกจุด

**AI เขียนโค้ดให้ (boilerplate เท่านั้น)** — `ReplayFileIO.cs` (`Save`/`Load` ผ่าน `JsonUtility`) และเพิ่ม `[System.Serializable]` ให้ `Replay` struct เพราะเป็นแค่ I/O plumbing ไม่ใช่ core sim logic — ตัดสินใจเลือก JSON แทน ScriptableObject เองก่อน (เพราะ ScriptableObject โหลดไม่ได้ถ้าไม่มี Unity Player ขัดกับ requirement ของ option นี้โดยตรง) แล้วให้ AI ช่วยเขียนโค้ด I/O ตามที่ตัดสินใจไว้

**รอบ ServerSimulator + bug เพิ่มเติม** — ผมเสนอเองว่าอยากให้มี script แยกจำลอง "ส่ง request" (claimed score + replay) ไปเทียบกับคะแนนที่ server คำนวณเอง ตรงกับ design ใน Part 1.2 ผมออกแบบ/เขียน `ServerSimulatorEditor.cs` เอง ระหว่างนั้น AI ชี้ 2 จุด: (1) ต้อง expose `Score` เป็น public getter ก่อนถึงจะอ่านค่าจากภายนอกได้ (2) bug ร้ายแรงจาก logic เงื่อนไขกลับด้านใน constructor ของ `RunnerSimulation` (`SimMode.Replay`) ที่ทำให้ path เดิม (`RunnerSimulationEditor`) พังด้วย NullReferenceException — ผมแก้เอง นอกจากนี้ AI แนะนำเพิ่ม obstacle checksum (เพื่อพิสูจน์ความ deterministic แน่นกว่าแค่ดู score) และแก้ปัญหา `Stopwatch.ElapsedMilliseconds` ปัดเป็น 0 (ใช้ `Elapsed.TotalMilliseconds` แทน) ซึ่งผมนำไปเขียนเอง

**รอบตรวจสอบสุดท้าย (Fable second opinion อีกรอบ)** — ให้ Claude ขอ Fable ตรวจทั้ง 5 part พร้อมกันอีกครั้งก่อนส่งจริง เจอว่าเอกสาร Part 1.1/Part 5 อ้างว่า "round float ทุก tick" เป็นทางแก้ float drift แต่โค้ด `BaseUpdater.cs` ตอนนั้นไม่มีการ round จริงเลย (พิสูจน์แค่ fixed-timestep เฉยๆ) — ผมเลือกแก้โค้ดจริงแทนแก้แค่คำในเอกสาร เพิ่ม `Math.Round` ใน `speed` เอง รอบแรกที่เขียนดัน round ผิดจุด (round ค่าคงที่ที่บวกเพิ่มแต่ละ tick แทนที่จะ round ตัว `speed` ที่สะสมไว้ ซึ่งไม่มีผลอะไรเพราะค่าคงที่ไม่มีทาง drift อยู่แล้ว) AI ชี้จุดนี้ ผมแก้เป็น round ตัว `speed` หลังบวกเสร็จแทน แล้วรัน demo ใหม่ทั้งหมด ผลยังตรงกันทุก framerate เหมือนเดิม (อัปเดตตัวเลขในตารางข้างบนเป็นชุดใหม่จากรอบนี้)

เนื้อหา/สถาปัตยกรรม/การตัดสินใจออกแบบทั้งหมดเป็นของผมเอง AI ไม่ได้ออกแบบหรือเขียน core logic แทน มีบทบาทแค่อธิบาย concept, ชี้ bug, และเขียน boilerplate ตามที่สั่ง
