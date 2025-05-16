# ExcelToXML
テーブル形式のメタデータを入力スキーマに従ってXMLファイルへ変換する。Convert table-format data to XML files following the input schema.

```mermaid
flowchart LR
A[サンプルXML] -->|XML構造| D[XML作成]
B[データテーブルA・B] -->|XML各要素の値| D
C[要素名定義テーブル] -->|データテーブルの各要素名とXPathの対応|D
D --> E[XML検証]
G[XSD検証] --> H[XSDファイル]
H -.-> E
```
