# Run after export-report.py on Windows with Microsoft Word installed.
# Updates the table of contents, checks body pagination, and exports the PDF.
$ErrorActionPreference = 'Stop'
$reportPath = Join-Path $PSScriptRoot 'bao-cao-tot-nghiep.docx'
$pdfPath = Join-Path $PSScriptRoot 'bao-cao-tot-nghiep.pdf'
$reportWord = New-Object -ComObject Word.Application
$reportDoc = $null
$reportWord.Visible = $false
$reportWord.DisplayAlerts = 0
try {
    $reportDoc = $reportWord.Documents.Open($reportPath, $false, $false, $false)
    # Word creates its built-in TOC styles when the field is first expanded.
    $reportDoc.Fields.Update() | Out-Null
    foreach ($styleId in @(-20, -21)) {
        $tocStyle = $reportDoc.Styles.Item($styleId)
        $tocStyle.Font.Name = 'Times New Roman'
        $tocStyle.Font.Size = 12
        $tocStyle.ParagraphFormat.LineSpacingRule = 0
        $tocStyle.ParagraphFormat.SpaceBefore = 0
        $tocStyle.ParagraphFormat.SpaceAfter = 2
    }
    foreach ($toc in $reportDoc.TablesOfContents) { $toc.Update() }
    $reportDoc.Repaginate()
    foreach ($toc in $reportDoc.TablesOfContents) { $toc.UpdatePageNumbers() }
    $reportDoc.Repaginate()
    $first = $reportDoc.Bookmarks.Item('ReportBodyStart').Range.Information(3)
    $last = $reportDoc.Bookmarks.Item('ReportBodyEnd').Range.Information(3)
    $bodyPages = $last - $first + 1
    Write-Output "Body: $bodyPages A4 pages (physical pages $first-$last)."
    if ($bodyPages -ne 12) {
        Write-Warning 'Body length differs from the 12-page target; inspect the PDF.'
    }
    $reportDoc.Save()
    $reportDoc.ExportAsFixedFormat($pdfPath, 17)
    Write-Output "Exported $pdfPath"
} finally {
    if ($null -ne $reportDoc) { $reportDoc.Close(0) }
    $reportWord.Quit()
    if ($null -ne $reportDoc) {
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($reportDoc)
    }
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($reportWord)
}
