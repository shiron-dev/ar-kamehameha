import cv2  # OpenCV:画像処理系ライブラリ
import dlib  # 機械学習系ライブラリ
import imutils  # OpenCVの補助
from imutils import face_utils
import numpy as np
from socket import socket, AF_INET, SOCK_DGRAM
import os

HOST = ''
PORT = 5000
ADDRESS = "127.0.0.1"

s = socket(AF_INET, SOCK_DGRAM)

# VideoCapture オブジェクトを取得します
DEVICE_ID = 0  # ID 0は標準web cam
capture = cv2.VideoCapture(DEVICE_ID)  # dlibの学習済みデータの読み込み
predictor_path = os.path.join(os.path.dirname(__file__), "data/shape_predictor_68_face_landmarks.dat")

detector = dlib.get_frontal_face_detector()  # 顔検出器の呼び出し。ただ顔だけを検出する。
predictor = dlib.shape_predictor(predictor_path)  # 顔から目鼻などランドマークを出力する

while (True):  # カメラから連続で画像を取得する
    ret, frame = capture.read()  # カメラからキャプチャしてframeに１コマ分の画像データを入れる

    frame = imutils.resize(frame, width=1000)  # frameの画像の表示サイズを整える
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)  # gray scaleに変換する
    rects = detector(gray, 0)  # grayから顔を検出
    image_points = None

    for rect in rects:
        shape = predictor(gray, rect)
        shape = face_utils.shape_to_np(shape)

        for (x, y) in shape:  # 顔全体の68箇所のランドマークをプロット
            cv2.circle(frame, (x, y), 1, (255, 255, 255), -1)

        image_points = np.array([
            tuple(shape[30]),  # 鼻頭
            tuple(shape[21]),
            tuple(shape[22]),
            tuple(shape[39]),
            tuple(shape[42]),
            tuple(shape[31]),
            tuple(shape[35]),
            tuple(shape[48]),
            tuple(shape[54]),
            tuple(shape[57]),
            tuple(shape[8]),
        ], dtype='double')

    if len(rects) > 0:
        model_points = np.array([
            (0.0, 0.0, 0.0),  # 30
            (-30.0, -125.0, -30.0),  # 21
            (30.0, -125.0, -30.0),  # 22
            (-60.0, -70.0, -60.0),  # 39
            (60.0, -70.0, -60.0),  # 42
            (-40.0, 40.0, -50.0),  # 31
            (40.0, 40.0, -50.0),  # 35
            (-70.0, 130.0, -100.0),  # 48
            (70.0, 130.0, -100.0),  # 54
            (0.0, 158.0, -10.0),  # 57
            (0.0, 250.0, -50.0)  # 8
        ])

        size = frame.shape

        focal_length = size[1]
        center = (size[1] // 2, size[0] // 2)  # 顔の中心座標

        camera_matrix = np.array([
            [focal_length, 0, center[0]],
            [0, focal_length, center[1]],
            [0, 0, 1]
        ], dtype='double')

        dist_coeffs = np.zeros((4, 1))

        (success, rotation_vector, translation_vector) = cv2.solvePnP(model_points, image_points, camera_matrix,
                                                                      dist_coeffs, flags=cv2.SOLVEPNP_ITERATIVE)
        # 回転行列とヤコビアン
        (rotation_matrix, jacobian) = cv2.Rodrigues(rotation_vector)
        mat = np.hstack((rotation_matrix, translation_vector))

        # yaw,pitch,rollの取り出し
        (_, _, _, _, _, _, eulerAngles) = cv2.decomposeProjectionMatrix(mat)
        yaw = eulerAngles[1]
        pitch = eulerAngles[0]
        roll = eulerAngles[2]

        print("yaw", int(yaw), "pitch", int(pitch), "roll", int(roll))  # 頭部姿勢データの取り出し

        cv2.putText(frame, 'yaw : ' + str(int(yaw)), (20, 10), cv2.FONT_HERSHEY_PLAIN, 1, (0, 0, 255), 2)
        cv2.putText(frame, 'pitch : ' + str(int(pitch)), (20, 25), cv2.FONT_HERSHEY_PLAIN, 1, (0, 0, 255), 2)
        cv2.putText(frame, 'roll : ' + str(int(roll)), (20, 40), cv2.FONT_HERSHEY_PLAIN, 1, (0, 0, 255), 2)

        (nose_end_point2D, _) = cv2.projectPoints(np.array([(0.0, 0.0, 500.0)]), rotation_vector,
                                                  translation_vector, camera_matrix, dist_coeffs)
        # 計算に使用した点のプロット/顔方向のベクトルの表示
        for i, p in enumerate(image_points):
            cv2.drawMarker(frame, (int(p[0]), int(p[1])), (0.0, 1.409845, 255), markerType=cv2.MARKER_CROSS,
                           thickness=1)

            s.sendto('{}:{},{}'.format(i, int(p[0]), int(p[1])).encode(), (ADDRESS, PORT))

        p1 = (int(image_points[0][0]), int(image_points[0][1]))
        p2 = (int(nose_end_point2D[0][0][0]), int(nose_end_point2D[0][0][1]))

        cv2.arrowedLine(frame, p1, p2, (255, 0, 0), 2)

    cv2.imshow('frame', frame)  # 画像を表示する
    if cv2.waitKey(1) & 0xFF == ord('q'):  # qを押すとbreakしてwhileから抜ける
        break

capture.release()  # video captureを終了する
cv2.destroyAllWindows()  # windowを閉じる
s.close()
