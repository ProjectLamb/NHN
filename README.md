# SandwichOS AI Sandwich Game

Unity 2023.2.22f1 WebGL 프로젝트다. `GameStartMenu`의 기존 연출과 `GameScene`의 기존 UI를 유지하면서, TMP 명령 입력을 Firebase Functions 2세대와 OpenAI Responses API에 연결한다.

## 현재 씬 자동 설정

처음 스크립트를 가져온 뒤 Unity의 Play Mode를 종료하면 누락된 데이터베이스를 감지하여 한 번 자동 설정한다. 자동 설정은 열린 씬을 저장하고 `GameScene`을 구성한 뒤 원래 씬으로 돌아온다. 수동 재적용은 다음 메뉴를 사용한다.

`Sandwich > Apply Complete Setup To GameScene`

이 작업이 생성/연결하는 항목:

- PDF 기반 상태 전이 49개와 stateId 51개
- `SandwichGameSystem`, 재료 슬롯 6개, `SandwichLayerRoot`
- 기존 `InputField (TMP)`와 `Button (Legacy)` 재사용
- 상태 메시지 TMP 텍스트 및 버튼 Submit 이벤트
- `AICommandManager.useMockResponse = true` 기본값

현재 양배추와 토마토는 `Assets/Resources`의 실제 FBX 및 씬 오브젝트에 연결되어 있다. 다른 재료는 실제 모델이 준비되면 `Assets/ScriptableObjects/IngredientPrefabDatabase.asset`에서 같은 stateId 행에 연결한다.

## Mock 테스트

GameScene에서 Play 후 다음 명령을 입력하고 RUN 버튼을 누른다.

`빵 봉지를 열고 빵을 꺼내서 얇게 잘라`

기본 빵 테스트에서는 Mock이 `Open bread`, `TakeOff bread M`, `Cut bread S`를 순서대로 실행한다.

양배추는 Mock에서도 실제 입력을 인식한다. `양배추를 작게 잘라`, `양배추를 적당히 잘라`, `양배추를 크게 잘라`를 입력하면 각각 Cut S/M/L로 처리되고 씬의 양배추가 `양배추_조각.fbx`로 교체된다.

## 남은 게임 설정

- `SandwichGameSystem > SandwichValidator`에 최종 정답 레시피의 재료/stateId 순서를 입력한다.
- 임시 Cube를 실제 재료 프리팹으로 교체한다.
- 실제 서버를 쓸 때 `AICommandManager`의 `Use Mock Response`를 끄고 Function URL을 입력한다.

## Firebase/OpenAI 설정

Cloud Functions 배포에는 Blaze 요금제와 `OPENAI_API_KEY` Secret이 필요하다. 로컬 Mock은 필요 없다.

```powershell
cd Server/functions
npm install
npm run build
cd ..
firebase login
firebase use YOUR_FIREBASE_PROJECT_ID
firebase functions:secrets:set OPENAI_API_KEY
firebase deploy --only functions:interpretSandwichCommand
```

`Server/.firebaserc`의 프로젝트 ID를 바꾸고, `Server/functions/.env.YOUR_FIREBASE_PROJECT_ID`를 로컬에 만든다.

```dotenv
OPENAI_MODEL=gpt-5.6-sol
ALLOWED_ORIGINS=https://YOUR_WEBGL_HOST.example,http://localhost:8080
```

Emulator:

```powershell
cd Server
firebase emulators:start --only functions
```

일반적인 로컬 URL은 `http://127.0.0.1:5001/PROJECT_ID/asia-northeast3/interpretSandwichCommand`다.

## WebGL

Build Settings에서 WebGL로 전환하고 `GameStartMenu`, `GameScene` 순서가 유지되는지 확인한다. HTTPS로 호스팅할 때 Function URL도 HTTPS여야 하며 실제 호스팅 Origin을 `ALLOWED_ORIGINS`에 추가해야 한다.

## Git

커밋해야 하는 것: `Assets`와 `.meta`, `Packages`, `ProjectSettings`, `Server/functions/src`, Firebase 설정 파일.

커밋하지 않는 것: `Library`, `Temp`, `Logs`, `UserSettings`, `Build`, `node_modules`, `Server/functions/lib`, `.env*`, API 키와 서비스 계정 JSON.
