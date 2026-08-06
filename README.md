# SandwichOS AI Sandwich Game

Unity 2023.2.22f1 WebGL 프로젝트입니다. 게임의 TMP 명령 입력을 Cloudflare Worker를 통해 OpenAI Responses API에 연결합니다. OpenAI API 키는 Unity 빌드에 포함하지 않고 Worker Secret으로 보관합니다.

## Unity 자동 설정

Unity에서 Play Mode를 종료하면 필요한 데이터베이스와 오브젝트를 자동 설정합니다. 수동으로 다시 적용하려면 다음 메뉴를 사용합니다.

`Sandwich > Apply Complete Setup To GameScene`

실제 서버를 사용하려면 `GameScene`의 `AICommandManager`에서 다음 값을 설정합니다.

- `Use Mock Response`: 끔
- `Function URL`: 배포 후 출력되는 `https://sandwich-command-api.<subdomain>.workers.dev`

## Cloudflare Worker 설정 및 배포

요구 사항은 Node.js, pnpm, 무료 Cloudflare 계정입니다. pnpm이 없다면 먼저 `corepack enable`로 활성화합니다.

```powershell
cd Server/functions
pnpm install
pnpm exec wrangler login
pnpm exec wrangler secret put OPENAI_API_KEY
pnpm deploy
```

`secret put` 명령이 값을 물으면 OpenAI API 키를 붙여 넣습니다. 키는 `wrangler.jsonc`나 Unity 프로젝트에 기록하지 않습니다.

배포 후 터미널에 표시되는 Worker URL을 `Assets/Scenes/GameScene.unity`의 `AICommandManager > Function URL`에 입력합니다. 코드 기본값에 있는 `YOUR_SUBDOMAIN`도 실제 Cloudflare 서브도메인으로 바꿀 수 있습니다.

## 허용할 WebGL 주소 설정

`Server/functions/wrangler.jsonc`의 `ALLOWED_ORIGINS`에 실제 WebGL 호스팅 주소를 쉼표로 추가합니다. Origin은 경로 없이 스킴과 호스트만 적습니다.

```json
"ALLOWED_ORIGINS": "https://game.example.com,http://localhost:8080"
```

변경 후 `pnpm deploy`를 다시 실행합니다. Unity 에디터처럼 `Origin` 헤더가 없는 요청은 허용되지만, 브라우저 WebGL 요청은 목록에 있는 Origin만 허용됩니다.

## 로컬 실행

`Server/functions/.dev.vars` 파일을 만들고 아래처럼 입력합니다. 이 파일은 Git에서 제외됩니다.

```dotenv
OPENAI_API_KEY=YOUR_OPENAI_API_KEY
```

그다음 실행합니다.

```powershell
cd Server/functions
pnpm dev
```

기본 로컬 주소는 Wrangler가 터미널에 출력합니다. 로컬 WebGL 테스트 시 그 주소를 Unity의 Function URL로 사용합니다.

## 점검

```powershell
cd Server/functions
pnpm test
```

OpenAI API 사용료는 Cloudflare와 별개입니다. Cloudflare Workers 무료 한도를 초과하면 무료 플랜에서는 추가 요청이 실패하며 자동으로 유료 과금되지 않습니다.

## Git 제외 대상

`Library`, `Temp`, `Logs`, `UserSettings`, `Build`, `node_modules`, `.dev.vars`, API 키를 커밋하지 않습니다.
