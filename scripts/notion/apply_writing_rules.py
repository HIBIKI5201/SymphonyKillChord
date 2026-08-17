"""モジュール文書へ「仕様書 ライティング規則」を適用する。

規則:
  - 口調はである調
  - 文末には句点。ただしリストの文章と括弧内の文章には付けない
  - H1には背景色（背景色の付与はNotionへの書き込み時に行う）
"""
import io
import re
import sys

# 長いものから順に適用する。五段・一段動詞をサ変の一括変換より先に置く。
RULES = [
    ("ていません", "ていない"),
    ("ています", "ている"),
    ("ありません", "ない"),
    ("あります", "ある"),
    ("できません", "できない"),
    ("できます", "できる"),
    ("起きません", "起きない"),
    ("しまいます", "しまう"),
    ("持ちます", "持つ"),
    ("取ります", "取る"),
    ("戻ります", "戻る"),
    ("直します", "直す"),
    ("移します", "移す"),
    ("戻します", "戻す"),
    ("映します", "映す"),
    ("出します", "出す"),
    ("示します", "示す"),
    ("続けます", "続ける"),
    ("受けます", "受ける"),
    ("扱えます", "扱える"),
    ("揃えます", "揃える"),
    ("求めます", "求める"),
    ("決めます", "決める"),
    ("替えます", "替える"),
    ("付けます", "付ける"),
    ("使います", "使う"),
    ("担います", "担う"),
    ("扱います", "扱う"),
    ("行います", "行う"),
    ("呼びます", "呼ぶ"),
    ("なります", "なる"),
    ("させます", "させる"),
    ("されません", "されない"),
    ("されます", "される"),
    ("われます", "われる"),
    ("しません", "しない"),
    ("します", "する"),
    ("ません", "ない"),
    ("でしょう", "だろう"),
    ("でした", "だった"),
    ("です", "である"),
]

# 「反映します」「検出します」のような漢字のサ変複合語。五段動詞のルールより優先して守る。
AMBIGUOUS_PATTERN = re.compile(r"([一-龥])(出|示|映|移|戻|直)(?=し(ます|ません))")
LIST_PATTERN = re.compile(r"^(\s*([-*+]|\d+\.)\s+.*?)。\s*$")
PAREN_PATTERN = re.compile(r"。(?=[)）])")


def convert(text: str) -> str:
    lines = text.replace("\r\n", "\n").split("\n")
    out = []
    in_code = False
    for line in lines:
        if line.lstrip().startswith("```"):
            in_code = not in_code
            out.append(line)
            continue

        if in_code:
            out.append(line)
            continue

        # 「反映します」のような漢字のサ変複合語を、五段動詞のルールで削らないよう保護する。
        line = AMBIGUOUS_PATTERN.sub(lambda m: m.group(1) + m.group(2) + "\x00", line)
        for old, new in RULES:
            line = line.replace(old, new)
        line = line.replace("\x00します", "する").replace("\x00しません", "しない").replace("\x00", "")

        # 括弧内の文章には句点を付けない。
        line = PAREN_PATTERN.sub("", line)

        # リストの文章には句点を付けない。
        match = LIST_PATTERN.match(line)
        if match:
            line = match.group(1)

        out.append(line)

    return "\n".join(out)


def main() -> int:
    changed = 0
    for path in sys.argv[1:]:
        source = io.open(path, encoding="utf-8").read()
        converted = convert(source)
        if converted != source:
            io.open(path, "w", encoding="utf-8", newline="").write(converted)
            changed += 1
            print("適用:", path)
    print(f"{changed}件を更新した")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
