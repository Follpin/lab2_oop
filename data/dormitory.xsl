<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" encoding="utf-8" doctype-public="-//W3C//DTD HTML 4.01 Transitional//EN" />

  <xsl:template match="/">
    <html lang="uk">
      <head>
        <meta charset="utf-8" />
        <title>Звіт про мешканців гуртожитку - Варіант 3</title>
        <style>
          body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #ffffff;
            color: #222222;
            margin: 24px;
            font-size: 13px;
          }
          .header-container {
            border-bottom: 2px solid #2b579a;
            padding-bottom: 12px;
            margin-bottom: 16px;
          }
          .header-container h1 {
            margin: 0 0 6px 0;
            font-size: 20px;
            color: #222222;
          }
          .header-container p {
            margin: 3px 0;
            font-size: 13px;
            color: #444444;
          }
          .stats-bar {
            display: flex;
            gap: 24px;
            margin-bottom: 16px;
            padding: 10px 14px;
            background-color: #f7f7f7;
            border: 1px solid #d8d8d8;
          }
          .stat-card {
            display: flex;
            gap: 6px;
            align-items: baseline;
          }
          .stat-title {
            font-size: 13px;
            color: #555555;
          }
          .stat-title::after {
            content: ":";
          }
          .stat-value {
            font-size: 14px;
            font-weight: bold;
            color: #222222;
          }
          table {
            width: 100%;
            border-collapse: collapse;
          }
          th, td {
            border: 1px solid #d4d4d4;
            padding: 8px 10px;
            text-align: left;
            font-size: 13px;
          }
          th {
            background-color: #efefef;
            color: #222222;
            font-weight: 600;
          }
          tr:nth-child(even) {
            background-color: #fafafa;
          }
          .badge-none {
            color: #777777;
          }
          .footer {
            margin-top: 18px;
            font-size: 12px;
            color: #666666;
          }
        </style>
      </head>
      <body>
        <div class="header-container">
          <h1>База даних гуртожитку - Звіт мешканців</h1>
          <p><strong>Університет:</strong> <xsl:value-of select="/DormitoryDatabase/@University" /></p>
          <p><strong>Навчальний рік:</strong> <xsl:value-of select="/DormitoryDatabase/@AcademicYear" /></p>
          <p><strong>Виконав:</strong> студент групи К-24 Остапенко Роман (Варіант 3)</p>
        </div>

        <div class="stats-bar">
          <div class="stat-card">
            <div class="stat-title">Всього студентів</div>
            <div class="stat-value"><xsl:value-of select="count(/DormitoryDatabase/Student)" /></div>
          </div>
          <div class="stat-card">
            <div class="stat-title">Мають пільги</div>
            <div class="stat-value"><xsl:value-of select="count(/DormitoryDatabase/Student[@Privilege])" /></div>
          </div>
          <div class="stat-card">
            <div class="stat-title">Вказано телефон</div>
            <div class="stat-value"><xsl:value-of select="count(/DormitoryDatabase/Student[@Phone])" /></div>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>П.І.П. студента</th>
              <th>Факультет</th>
              <th>Кафедра</th>
              <th>Курс</th>
              <th>Корпус / Кімната</th>
              <th>Період проживання</th>
              <th>Плата (грн/міс)</th>
              <th>Пільга</th>
              <th>Телефон</th>
            </tr>
          </thead>
          <tbody>
            <xsl:for-each select="/DormitoryDatabase/Student">
              <xsl:sort select="@Faculty" />
              <xsl:sort select="@Course" data-type="number" />
              <tr>
                <td><strong><xsl:value-of select="@Id" /></strong></td>
                <td><xsl:value-of select="@FullName" /></td>
                <td><xsl:value-of select="@Faculty" /></td>
                <td><xsl:value-of select="@Department" /></td>
                <td>
                  <span class="badge badge-course"><xsl:value-of select="@Course" /> курс</span>
                </td>
                <td>
                  <xsl:value-of select="Room/@Building" />, кімн. <xsl:value-of select="Room/@RoomNumber" />
                  (пов. <xsl:value-of select="Room/@Floor" />)
                </td>
                <td>
                  <xsl:choose>
                    <xsl:when test="Room/@CheckInDate and Room/@CheckOutDate and string-length(normalize-space(Room/@CheckOutDate)) &gt; 0">
                      <xsl:value-of select="Room/@CheckInDate" /> - <xsl:value-of select="Room/@CheckOutDate" />
                    </xsl:when>
                    <xsl:when test="Room/@CheckInDate and string-length(normalize-space(Room/@CheckInDate)) &gt; 0">
                      <xsl:value-of select="Room/@CheckInDate" /> - дотепер
                    </xsl:when>
                    <xsl:otherwise>
                      <span class="badge-none">Не вказано</span>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
                <td>
                  <xsl:value-of select="Room/@MonthlyFee" />
                </td>
                <td>
                  <xsl:choose>
                    <xsl:when test="@Privilege">
                      <span class="badge badge-privilege"><xsl:value-of select="@Privilege" /></span>
                    </xsl:when>
                    <xsl:otherwise>
                      <span class="badge-none">-</span>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
                <td>
                  <xsl:choose>
                    <xsl:when test="@Phone">
                      <xsl:value-of select="@Phone" />
                    </xsl:when>
                    <xsl:otherwise>
                      <span class="badge-none">-</span>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
              </tr>
            </xsl:for-each>
          </tbody>
        </table>

        <div class="footer">
          Згенеровано автоматично засобами XslCompiledTransform (.NET 8). Лабораторна робота №2 з ООП.
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
