from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate, Frame, PageTemplate, Paragraph, Spacer, Table, TableStyle,
    PageBreak, KeepTogether
)

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "output" / "pdf" / "team_member_roles_submission.pdf"
OUT.parent.mkdir(parents=True, exist_ok=True)

font_candidates = [
    Path(r"C:\Windows\Fonts\malgun.ttf"),
    Path(r"C:\Windows\Fonts\gulim.ttc"),
]
bold_candidates = [
    Path(r"C:\Windows\Fonts\malgunbd.ttf"),
    Path(r"C:\Windows\Fonts\gulim.ttc"),
]
font_path = next(p for p in font_candidates if p.exists())
bold_path = next(p for p in bold_candidates if p.exists())
pdfmetrics.registerFont(TTFont("KR", str(font_path)))
pdfmetrics.registerFont(TTFont("KR-Bold", str(bold_path)))

PAGE_W, PAGE_H = A4
NAVY = colors.HexColor("#17243A")
BLUE = colors.HexColor("#356AE6")
PALE = colors.HexColor("#EEF3FF")
LIGHT = colors.HexColor("#F5F7FA")
MUTED = colors.HexColor("#667085")
LINE = colors.HexColor("#D8DEE9")

styles = getSampleStyleSheet()
title = ParagraphStyle("title", fontName="KR-Bold", fontSize=24, leading=32,
                       textColor=colors.white, alignment=TA_CENTER, spaceAfter=8)
subtitle = ParagraphStyle("subtitle", fontName="KR", fontSize=10, leading=16,
                          textColor=colors.HexColor("#DCE5FF"), alignment=TA_CENTER)
h1 = ParagraphStyle("h1", fontName="KR-Bold", fontSize=16, leading=22,
                    textColor=NAVY, spaceBefore=4, spaceAfter=10)
h2 = ParagraphStyle("h2", fontName="KR-Bold", fontSize=12, leading=17,
                    textColor=BLUE, spaceBefore=4, spaceAfter=5)
body = ParagraphStyle("body", fontName="KR", fontSize=9.3, leading=15,
                      textColor=colors.HexColor("#273142"), spaceAfter=4)
small = ParagraphStyle("small", fontName="KR", fontSize=8, leading=12,
                       textColor=MUTED)
cell = ParagraphStyle("cell", fontName="KR", fontSize=8.5, leading=13,
                      textColor=colors.HexColor("#273142"))
cell_bold = ParagraphStyle("cell_bold", parent=cell, fontName="KR-Bold", textColor=NAVY)


def P(text, style=body):
    return Paragraph(text, style)


def bullet(text):
    return P("• " + text, body)


def header_footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(LINE)
    canvas.line(18 * mm, 15 * mm, PAGE_W - 18 * mm, 15 * mm)
    canvas.setFont("KR", 7.5)
    canvas.setFillColor(MUTED)
    canvas.drawString(18 * mm, 9.5 * mm, "팀원 롤 기술서 | GitHub 및 설계 산출물 기반")
    canvas.drawRightString(PAGE_W - 18 * mm, 9.5 * mm, str(doc.page))
    canvas.restoreState()


doc = BaseDocTemplate(
    str(OUT), pagesize=A4,
    leftMargin=18 * mm, rightMargin=18 * mm,
    topMargin=17 * mm, bottomMargin=21 * mm,
    title="팀원 롤 기술서", author="Project LAMB"
)
frame = Frame(doc.leftMargin, doc.bottomMargin, doc.width, doc.height, id="main")
doc.addPageTemplates(PageTemplate(id="all", frames=[frame], onPage=header_footer))

story = []

hero = Table([[P("팀원 롤 기술서", title),],
              [P("Project LAMB · 역할 및 실제 구현 영역 정리", subtitle)]],
             colWidths=[doc.width], rowHeights=[22 * mm, 13 * mm])
hero.setStyle(TableStyle([
    ("BACKGROUND", (0, 0), (-1, -1), NAVY),
    ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
    ("LEFTPADDING", (0, 0), (-1, -1), 10),
    ("RIGHTPADDING", (0, 0), (-1, -1), 10),
    ("TOPPADDING", (0, 0), (-1, -1), 3),
    ("BOTTOMPADDING", (0, 0), (-1, -1), 3),
]))
story += [hero, Spacer(1, 9 * mm)]

story += [P("1. 팀 구성 및 역할", h1)]
summary_data = [
    [P("팀원", cell_bold), P("담당 역할", cell_bold), P("핵심 책임", cell_bold)],
    [P("이진명<br/><font color='#667085'>팀장</font>", cell_bold), P("기획 · 아트", cell),
     P("게임 콘셉트와 플레이 흐름 설계, 비주얼 방향 수립, UI·그래픽 및 3D 아트 제작·검수", cell)],
    [P("김종원", cell_bold), P("개발 · AI 활용", cell),
     P("Unity 핵심 시스템, 자연어 명령 처리, AI API·서버 연동, 게임 규칙 및 WebGL 대응", cell)],
    [P("장문수", cell_bold), P("개발 · AI 활용", cell),
     P("인게임 상태 데이터 구조와 AI 시스템 프롬프트 설계, UI·재료 프리팹·게임 씬 및 연출 통합", cell)],
]
summary = Table(summary_data, colWidths=[29 * mm, 35 * mm, doc.width - 64 * mm], repeatRows=1)
summary.setStyle(TableStyle([
    ("BACKGROUND", (0, 0), (-1, 0), PALE),
    ("GRID", (0, 0), (-1, -1), .5, LINE),
    ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ("LEFTPADDING", (0, 0), (-1, -1), 7),
    ("RIGHTPADDING", (0, 0), (-1, -1), 7),
    ("TOPPADDING", (0, 0), (-1, -1), 7),
    ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
]))
story += [summary, Spacer(1, 7 * mm)]

story += [P("2. 팀원별 실제 수행 영역", h1)]

lee = [
    P("이진명 (팀장) — 기획 · 아트", h2),
    bullet("샌드위치 조리 과정을 자연어 명령으로 수행한다는 핵심 게임 콘셉트와 사용자 경험을 기획했습니다."),
    bullet("메뉴, 레시피, 조리 화면, 로봇·터미널 콘셉트 등 전체 비주얼 방향과 화면 구성을 설계했습니다."),
    bullet("UI 이미지, 재료 및 주방 관련 아트 리소스의 제작·선정·수정 방향을 관리하고 게임 적용 결과를 검수했습니다."),
    bullet("팀장으로서 기능 우선순위, 일정, 기획-아트-개발 간 요구사항을 조율하고 최종 제출 품질을 관리했습니다."),
]
story += [KeepTogether(lee), Spacer(1, 4 * mm)]

kim = [
    P("김종원 — 개발 · AI 활용", h2),
    bullet("Unity 프로젝트와 샌드위치 조리 시스템의 기반 구조를 구축했습니다."),
    bullet("재료 상태 머신, Open/TakeOff/Cut/Put/Finish 액션 실행기, 샌드위치 적층·검증 로직을 구현하고 개선했습니다."),
    bullet("OpenAI 기반 자연어 명령을 구조화된 조리 액션으로 변환하도록 API 서버, 요청·응답 스키마, 시스템 프롬프트를 연동했습니다."),
    bullet("타이머·점수·메뉴 제시 시스템, 힌트 표시, WebGL 한글 입력, 빌드 리소스 최적화와 주요 버그 수정을 담당했습니다."),
]
story += [KeepTogether(kim), Spacer(1, 4 * mm)]

story.append(PageBreak())

jang = [
    P("장문수 — 개발 · AI 활용", h2),
    P("• 빵·햄·토마토·치즈·마요네즈·양배추의 상태 조합과 [양]_[행동]_[재료]_[상태] 형식의 상태 ID 네이밍 규칙을 설계했습니다.<br/><br/>"
      "• Open/TakeOff/Cut/Put/Finish 해석 규칙, amount 표준화, sourceStateId와 JSON 출력 형식, 정상·실패 예시를 포함한 OpenAI 시스템 프롬프트 초안을 작성했습니다.<br/><br/>"
      "• 게임 시작 메뉴와 게임 씬의 기본 구조, 로딩·메뉴 UI 및 화면 전환 기능을 구현했습니다.<br/><br/>"
      "• 명령 입력 UI, 원격 커서, 레시피 화면 등 실제 플레이 인터페이스를 구성하고 아트 리소스를 Unity 씬에 통합했습니다.<br/><br/>"
      "• 빵·양배추·토마토·햄·치즈·마요네즈 재료 프리팹과 조리용 에셋을 정리하고 장면에 배치했습니다.<br/><br/>"
      "• AI가 반환한 액션이 게임 화면에서 자연스럽게 표현되도록 액션 실행 연출과 최종 씬·UI 마무리 작업에 참여했습니다.", body),
]
story += [P("2. 팀원별 실제 수행 영역 (계속)", h1), KeepTogether(jang), Spacer(1, 7 * mm)]

story += [P("3. 협업 및 분업 방식", h1)]
collab_data = [
    [P("단계", cell_bold), P("협업 방식", cell_bold)],
    [P("기획·디자인", cell_bold), P("이진명이 게임 규칙, 플레이 흐름, 화면 콘셉트와 아트 방향을 정리하고 개발 요구사항으로 공유했습니다.", cell)],
    [P("AI·데이터 설계", cell_bold), P("장문수가 재료 상태 ID·전이표와 시스템 프롬프트 초안을 설계하고, 김종원이 이를 JSON Schema·서버·Unity 실행 파이프라인으로 구현·연동했습니다.", cell)],
    [P("화면·콘텐츠 통합", cell_bold), P("장문수가 UI, 프리팹, 씬과 연출을 구현해 AI 명령 시스템을 실제 플레이 가능한 형태로 통합했습니다.", cell)],
    [P("검수·개선", cell_bold), P("세 팀원이 플레이 테스트를 함께 진행하고 명령 해석 오류, 재료 적층, 화면 구성과 빌드 문제를 반복 수정했습니다.", cell)],
]
collab = Table(collab_data, colWidths=[35 * mm, doc.width - 35 * mm], repeatRows=1)
collab.setStyle(TableStyle([
    ("BACKGROUND", (0, 0), (-1, 0), PALE),
    ("BACKGROUND", (0, 1), (0, -1), LIGHT),
    ("GRID", (0, 0), (-1, -1), .5, LINE),
    ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ("LEFTPADDING", (0, 0), (-1, -1), 7),
    ("RIGHTPADDING", (0, 0), (-1, -1), 7),
    ("TOPPADDING", (0, 0), (-1, -1), 7),
    ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
]))
story += [collab, Spacer(1, 7 * mm)]

story += [P("4. 기획·설계 산출물 근거", h1)]
artifact_data = [
    [P("산출물", cell_bold), P("장문수 기여 내용", cell_bold)],
    [P("인게임 데이터 구조 (4쪽)", cell_bold), P("재료별 화면 상태·조합·상태 ID를 표로 정의하고, 빵·햄·토마토·치즈·마요네즈·양배추의 상태 전이 기준과 네이밍 규칙을 설계", cell)],
    [P("OpenAI에 넣을 시스템 프롬프트 (8쪽)", cell_bold), P("명시 행동만 실행하는 원칙, 허용 action·재료·amount, 자연어 매핑, sourceStateId, JSON 형식과 정상·실패 테스트 예시를 설계", cell)],
]
artifact = Table(artifact_data, colWidths=[58 * mm, doc.width - 58 * mm], repeatRows=1)
artifact.setStyle(TableStyle([
    ("BACKGROUND", (0, 0), (-1, 0), PALE),
    ("GRID", (0, 0), (-1, -1), .5, LINE),
    ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ("LEFTPADDING", (0, 0), (-1, -1), 7),
    ("RIGHTPADDING", (0, 0), (-1, -1), 7),
    ("TOPPADDING", (0, 0), (-1, -1), 7),
    ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
]))
story += [artifact, Spacer(1, 7 * mm)]

story += [P("5. GitHub 커밋 근거", h1)]
story += [
    P("2026-08-02부터 2026-08-09까지 현재 브랜치 HEAD의 커밋 이력을 검토했습니다. 커밋 작성자 기준으로 HobakVine 9건, Moonsu-Jang 5건, 초기 저장소 커밋 CarrotPancakeJJ 1건이 확인됩니다.", body),
]
evidence_data = [
    [P("커밋 계정", cell_bold), P("확인된 주요 변경 영역", cell_bold)],
    [P("HobakVine", cell_bold), P("Unity 프로젝트 생성, MOCK 명령 실행, OpenAI 연동, 타이머·점수, 조리 명령·재료 생성, 적층 버그, WebGL 한글 입력, 빌드 최적화", cell)],
    [P("Moonsu-Jang", cell_bold), P("게임 기본 세팅, 메뉴·로딩 UI, 게임 화면 UI, 재료 프리팹, 레시피·사운드·씬 통합, 최종 마무리. 별도 설계 문서로 상태 데이터와 시스템 프롬프트 기여 확인", cell)],
    [P("CarrotPancakeJJ", cell_bold), P("저장소 초기 생성 커밋", cell)],
]
evidence = Table(evidence_data, colWidths=[40 * mm, doc.width - 40 * mm], repeatRows=1)
evidence.setStyle(TableStyle([
    ("BACKGROUND", (0, 0), (-1, 0), PALE),
    ("GRID", (0, 0), (-1, -1), .5, LINE),
    ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ("LEFTPADDING", (0, 0), (-1, -1), 7),
    ("RIGHTPADDING", (0, 0), (-1, -1), 7),
    ("TOPPADDING", (0, 0), (-1, -1), 7),
    ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
]))
story += [evidence]

doc.build(story)
print(OUT)
