import tensorflow as tf
import cv2
import time
import argparse
from socket import socket, AF_INET, SOCK_DGRAM
import atexit

import posenet

parser = argparse.ArgumentParser()
parser.add_argument('--model', type=int, default=101)
parser.add_argument('--cam_id', type=int, default=0)
parser.add_argument('--cam_width', type=int, default=1280)
parser.add_argument('--cam_height', type=int, default=720)
parser.add_argument('--scale_factor', type=float, default=0.7125)
parser.add_argument('--file', type=str, default=None, help="Optionally use a video file instead of a live camera")
parser.add_argument('--ip', type=str, default="127.0.0.1")
parser.add_argument('--port', type=int, default=50001)
args = parser.parse_args()

# UDPセットアップ
udp_socket = socket(AF_INET, SOCK_DGRAM)


def send_udp(msg):
    # print(msg)
    udp_socket.sendto(msg.encode(), (args.ip, args.port))


def cleanup():
    udp_socket.close()


def main():
    with tf.Session() as sess:
        model_cfg, model_outputs = posenet.load_model(args.model, sess)
        output_stride = model_cfg['output_stride']

        if args.file is not None:
            cap = cv2.VideoCapture(args.file)
        else:
            cap = cv2.VideoCapture(args.cam_id)

        print(cap.get(cv2.CAP_PROP_FRAME_WIDTH),cap.get(cv2.CAP_PROP_FRAME_HEIGHT),)

        # cap.set(3, args.cam_width)
        # cap.set(4, args.cam_height)

        start = time.time()
        frame_count = 0
        while True:
            input_image, display_image, output_scale = posenet.read_cap(
                cap, scale_factor=args.scale_factor, output_stride=output_stride)

            heatmaps_result, offsets_result, displacement_fwd_result, displacement_bwd_result = sess.run(
                model_outputs,
                feed_dict={'image:0': input_image}
            )

            pose_scores, keypoint_scores, keypoint_coords = posenet.decode_multi.decode_multiple_poses(
                heatmaps_result.squeeze(axis=0),
                offsets_result.squeeze(axis=0),
                displacement_fwd_result.squeeze(axis=0),
                displacement_bwd_result.squeeze(axis=0),
                output_stride=output_stride,
                max_pose_detections=10,
                min_pose_score=0.15)

            keypoint_coords *= output_scale

            # 「nose」、「leftEye」、「rightEye」、「leftEar」、「rightEar」、「leftShoulder」、「rightShoulder」、「leftElbow」、「rightElbow」、「leftWrist」、「rightWrist」、「leftHip」、「rightHip」、「leftKnee」、「rightKnee」、「leftAnkle」、「rightAnkle」
            for i, pos in enumerate(keypoint_coords[0]):
                send_udp('{}:{},{}'.format(i, pos[1], pos[0]))

            # TODO this isn't particularly fast, use GL for drawing and display someday...
            overlay_image = posenet.draw_skel_and_kp(
                display_image, pose_scores, keypoint_scores, keypoint_coords,
                min_pose_score=0.15, min_part_score=0.1)

            cv2.imshow('posenet', overlay_image)
            frame_count += 1
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        print('Average FPS: ', frame_count / (time.time() - start))


if __name__ == "__main__":
    atexit.register(cleanup)
    main()
