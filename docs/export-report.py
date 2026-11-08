"""Export the report and source-derived diagrams.

Run: uv run --with python-docx --with pillow python docs/export-report.py
Then on Windows with Word: powershell -File docs/export-report-word.ps1

Markdown page markers are layout hints; use Word/PDF to verify actual pagination.
Diagrams describe source structure, not screenshots or runtime test evidence.
"""
from pathlib import Path
import math
import re
from PIL import Image, ImageDraw, ImageFont
from docx import Document
from docx.enum.section import WD_SECTION_START
from docx.shared import Cm, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

BASE = Path(__file__).resolve().parent
FIGURES = BASE / 'report-figures'


def diagrams():
    FIGURES.mkdir(exist_ok=True)
    font_dir = Path('C:/Windows/Fonts')
    font = ImageFont.truetype(str(font_dir / 'arial.ttf'), 34)
    small = ImageFont.truetype(str(font_dir / 'arial.ttf'), 29)
    bold = ImageFont.truetype(str(font_dir / 'arialbd.ttf'), 36)
    ink, accent = '#243345', '#355e63'

    def canvas(height):
        im = Image.new('RGB', (1600, height), 'white')
        return im, ImageDraw.Draw(im)

    def label(draw, xy, text, face=font, color=ink):
        draw.multiline_text(xy, text, font=face, fill=color, anchor='mm',
                            align='center', spacing=8)

    def box(draw, rect, text, face=font):
        draw.rounded_rectangle(rect, radius=14, fill='#f1f6f5', outline=accent, width=3)
        label(draw, ((rect[0]+rect[2])/2, (rect[1]+rect[3])/2), text, face)

    def arrow(draw, start, end, dashed=False):
        dx, dy = end[0]-start[0], end[1]-start[1]
        length = math.hypot(dx, dy)
        if dashed:
            for d in range(0, int(length), 22):
                a, b = d/length, min(d+12, length)/length
                draw.line((start[0]+dx*a, start[1]+dy*a,
                           start[0]+dx*b, start[1]+dy*b), fill=ink, width=3)
        else:
            draw.line((*start, *end), fill=ink, width=3)
        angle = math.atan2(dy, dx)
        points = [end] + [(end[0]-18*math.cos(angle+a), end[1]-18*math.sin(angle+a))
                          for a in (-0.45, 0.45)]
        draw.polygon(points, fill=ink)

    im, d = canvas(570)
    for y, actor, text in [(35, 'Khách truy cập', 'Xem thực đơn / bài viết\nCấu hình giỏ • Đặt COD • Tra cứu'),
                           (205, 'Thành viên', 'Các thao tác mua hàng của khách\nCập nhật hồ sơ • Đổi mật khẩu'),
                           (375, 'Quản trị viên', 'Quản lý thực đơn / bài viết / đơn\nQuản lý tài khoản và vai trò')]:
        box(d, (30,y,370,y+130),actor,bold)
        box(d, (500,y,1565,y+130),text)
        arrow(d,(370,y+65),(500,y+65))
    label(d,(800,545),'Sơ đồ tổng quát ca sử dụng theo nhóm tác nhân',small)
    im.save(FIGURES/'use-cases.png')

    im, d = canvas(630)
    for rect, text in [((25,45,330,155),'Trình duyệt'), ((470,45,870,155),'MVC Controllers'),
                       ((1080,45,1570,155),'Services / Repository'),
                       ((470,245,870,355),'Razor Views'),
                       ((1080,245,1570,355),'ApplicationDbContext'),
                       ((1080,465,1570,575),'SQL Server'),
                       ((30,465,425,575),'CartService → Session'),
                       ((510,465,995,575),'EmailService → SMTP')]:
        box(d,rect,text)
    arrow(d,(330,100),(470,100)); arrow(d,(870,100),(1080,100))
    arrow(d,(1325,155),(1325,245)); arrow(d,(1325,355),(1325,465))
    arrow(d,(670,155),(670,245)); arrow(d,(470,300),(175,155))
    arrow(d,(870,170),(1080,280),True)
    label(d,(1040,201),'trực tiếp',small)
    label(d,(800,407),'Dịch vụ còn sử dụng Session và SMTP như hai nhánh dưới',small)
    im.save(FIGURES/'architecture.png')

    im, d = canvas(620)
    for rect,text in [((25,40,390,160),'Category\nId • Name • Slug'),
                      ((570,40,1040,160),'Product\nId • Name • Price'),
                      ((35,300,520,420),'PropertyProduct\nId • Name • Price'),
                      ((1100,40,1570,160),'OptionProduct\nId • Name • Price'),
                      ((35,475,520,595),'Order\nCode • Total • Status • Paid'),
                      ((850,420,1570,595),'OrderDetail\nProductId • ProductName\nPrice • Quantity • Total')]:
        box(d,rect,text)
    for a,b,t,xy in [((390,100),(570,100),'1 : N',(480,70)),
                      ((1040,100),(1100,100),'1 : N',(1070,195)),
                      ((690,160),(400,300),'1 : N',(480,228)),
                      ((520,535),(850,535),'1 : N',(685,505))]:
        arrow(d,a,b); label(d,xy,t,small)
    arrow(d,(805,160),(1100,420),True)
    label(d,(1170,305),'ProductId lưu trong chi tiết\n(chưa xác nhận FK vật lý)',small)
    im.save(FIGURES/'data-model.png')

    im, d = canvas(700)
    xs=[125,455,795,1130,1470]
    for x,name in zip(xs,['Khách hàng','Controller','OrderService','SQL Server','EmailService']):
        box(d,(x-112,10,x+112,90),name,small)
        d.line((x,90,x,675),fill='#aab7bc',width=2)
    for y,a,b,text in [(150,0,1,'Gửi checkout'),(235,1,2,'CreateOrder'),
                        (320,2,3,'Lưu Order + chi tiết'),(400,3,2,'Kết quả lưu'),
                        (475,2,1,'Trả Order'),(555,1,4,'Thông báo nếu có email'),
                        (640,1,0,'Chuyển trang hoàn tất')]:
        arrow(d,(xs[a],y),(xs[b],y))
        label(d,((xs[a]+xs[b])/2,y-24),text,small)
    im.save(FIGURES/'checkout-sequence.png')


def field(paragraph, instruction):
    el = OxmlElement('w:fldSimple')
    el.set(qn('w:instr'), instruction)
    paragraph._p.append(el)


def bookmark(paragraph, name, ident):
    start = OxmlElement('w:bookmarkStart')
    start.set(qn('w:id'), str(ident)); start.set(qn('w:name'), name)
    end = OxmlElement('w:bookmarkEnd'); end.set(qn('w:id'), str(ident))
    paragraph._p.append(start); paragraph._p.append(end)


def export():
    diagrams()
    doc = Document()
    sec = doc.sections[0]
    sec.page_width, sec.page_height = Cm(21), Cm(29.7)
    sec.top_margin = sec.bottom_margin = Cm(2.5)
    sec.left_margin, sec.right_margin = Cm(3), Cm(2)
    for name, size in [('Normal',13),('Heading 1',16),('Heading 2',14),
                       ('Heading 3',13),('Caption',11),('Title',20),('Subtitle',14),
                       ('toc 1',12),('toc 2',12)]:
        if name not in doc.styles:
            doc.styles.add_style(name,1,builtin=name.startswith('toc'))
        st = doc.styles[name]
        st.font.name = 'Times New Roman'; st.font.size = Pt(size)
        fonts = st.element.get_or_add_rPr().get_or_add_rFonts()
        for attr in ('ascii','hAnsi','eastAsia','cs'):
            fonts.set(qn('w:'+attr),'Times New Roman')
        for attr in ('asciiTheme','hAnsiTheme','eastAsiaTheme','cstheme'):
            fonts.attrib.pop(qn('w:'+attr),None)
        st.font.color.rgb = RGBColor.from_string('000000')
        st.paragraph_format.line_spacing = 1.5
        st.paragraph_format.space_after = Pt(6)
        if name.startswith('Heading'):
            st.paragraph_format.space_before = Pt(8)
        if name.startswith('toc'):
            st.style_id = 'TOC' + name[-1]
            st.paragraph_format.line_spacing = 1.0
            st.paragraph_format.space_after = Pt(3)
    doc.styles['Normal'].paragraph_format.widow_control = True
    lines = (BASE/'bao-cao-tot-nghiep.md').read_text(encoding='utf-8').splitlines()
    pending_break = False
    in_body = False
    toc_list = False
    body_bookmark_pending = False
    i = 0

    def apply_break(p):
        nonlocal pending_break, body_bookmark_pending
        if pending_break:
            p.paragraph_format.page_break_before = True
            pending_break = False
        if body_bookmark_pending:
            bookmark(p,'ReportBodyStart',1)
            body_bookmark_pending = False

    while i < len(lines):
        line = lines[i].strip()
        i += 1
        if not line:
            continue
        if line == '<!-- body-start -->':
            sec = doc.add_section(WD_SECTION_START.NEW_PAGE)
            sec.footer.is_linked_to_previous = False
            num = OxmlElement('w:pgNumType'); num.set(qn('w:start'),'1')
            sec._sectPr.append(num)
            footer = sec.footer.paragraphs[0]; footer.alignment = 1
            field(footer,'PAGE')
            pending_break = False; in_body = True; body_bookmark_pending = True
            continue
        if line == '<!-- body-end -->':
            bookmark(doc.paragraphs[-1],'ReportBodyEnd',2)
            continue
        if line == '<!-- pagebreak -->':
            pending_break = True
            continue
        if line == '<!-- toc -->':
            field(doc.add_paragraph(),'TOC \\o "1-2" \\h \\z \\u')
            toc_list = True
            continue
        if toc_list and line.startswith('- '):
            continue
        toc_list = False
        if line.startswith('|'):
            rows=[]
            while True:
                cells=[x.strip() for x in line.strip('|').split('|')]
                if not all(re.fullmatch(r'[:\- ]+',x) for x in cells):
                    rows.append(cells)
                if i >= len(lines) or not lines[i].startswith('|'):
                    break
                line=lines[i]; i+=1
            table=doc.add_table(rows=0,cols=len(rows[0])); table.style='Table Grid'
            for n,row in enumerate(rows):
                tr=table.add_row()
                prop=tr._tr.get_or_add_trPr()
                prop.append(OxmlElement('w:cantSplit'))
                if n==0: prop.append(OxmlElement('w:tblHeader'))
                for cell,value in zip(tr.cells,row):
                    cell.text=value
                    for p in cell.paragraphs:
                        p.paragraph_format.line_spacing=1.0
                        p.paragraph_format.space_after=Pt(4)
                        p.paragraph_format.space_before=Pt(3)
                        for run in p.runs:
                            run.font.name='Times New Roman'
                            run.font.size=Pt(11); run.bold=n==0
            continue
        match=re.fullmatch(r'!\[(.+)\]\((.+)\)',line)
        if match:
            p=doc.add_paragraph(); apply_break(p); p.alignment=1
            p.paragraph_format.line_spacing=1
            p.paragraph_format.keep_with_next=True
            p.add_run().add_picture(str(BASE/match[2]),width=Cm(16))
            caption=doc.add_paragraph(match[1],style='Caption'); caption.alignment=1
            continue
        if line.startswith('#'):
            depth=len(line)-len(line.lstrip('#')); title=line.lstrip('#').strip()
            if not in_body:
                p=doc.add_paragraph(title,style='Title' if depth==1 else 'Subtitle')
                p.alignment=1
            else:
                p=doc.add_heading(title,level=min(depth,3))
            apply_break(p)
            continue
        p=doc.add_paragraph(line); p.alignment=3
        apply_break(p)
    update=OxmlElement('w:updateFields'); update.set(qn('w:val'),'true')
    doc.settings.element.append(update)
    target=BASE/'bao-cao-tot-nghiep.docx'
    doc.save(target)
    check=Document(target)
    print(f'Created {target}: {len(check.paragraphs)} paragraphs, '
          f'{len(check.tables)} tables, {len(check.inline_shapes)} figures')


if __name__ == '__main__':
    export()
