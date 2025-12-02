"""
돼지 피부 계측 데이터 분석 스크립트
CSV 파일에서 힘/토크 센서 데이터를 읽어 분석합니다.
"""

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
from pathlib import Path

# 한글 폰트 설정 (Windows)
plt.rcParams['font.family'] = 'Malgun Gothic'
plt.rcParams['axes.unicode_minus'] = False

def load_csv_data(filepath):
    """CSV 파일을 로드합니다."""
    try:
        df = pd.read_csv(filepath, header=None, names=['timestamp', 'elapsed_time', 'sample', 'X', 'Y', 'Z'])
        return df
    except Exception as e:
        print(f"파일 로드 오류 {filepath}: {e}")
        return None

def analyze_file(filepath):
    """단일 파일을 분석합니다."""
    df = load_csv_data(filepath)
    if df is None:
        return None
    
    filename = os.path.basename(filepath)
    
    analysis = {
        'filename': filename,
        'total_samples': len(df),
        'duration': df['elapsed_time'].max() - df['elapsed_time'].min(),
        'sampling_rate': len(df) / (df['elapsed_time'].max() - df['elapsed_time'].min()) if (df['elapsed_time'].max() - df['elapsed_time'].min()) > 0 else 0,
        'X': {
            'mean': df['X'].mean(),
            'std': df['X'].std(),
            'min': df['X'].min(),
            'max': df['X'].max(),
            'range': df['X'].max() - df['X'].min()
        },
        'Y': {
            'mean': df['Y'].mean(),
            'std': df['Y'].std(),
            'min': df['Y'].min(),
            'max': df['Y'].max(),
            'range': df['Y'].max() - df['Y'].min()
        },
        'Z': {
            'mean': df['Z'].mean(),
            'std': df['Z'].std(),
            'min': df['Z'].min(),
            'max': df['Z'].max(),
            'range': df['Z'].max() - df['Z'].min()
        },
        'magnitude': {
            'mean': np.sqrt(df['X']**2 + df['Y']**2 + df['Z']**2).mean(),
            'std': np.sqrt(df['X']**2 + df['Y']**2 + df['Z']**2).std(),
            'max': np.sqrt(df['X']**2 + df['Y']**2 + df['Z']**2).max()
        }
    }
    
    return analysis, df

def print_analysis(analysis):
    """분석 결과를 출력합니다."""
    print(f"\n{'='*60}")
    print(f"파일: {analysis['filename']}")
    print(f"{'='*60}")
    print(f"총 샘플 수: {analysis['total_samples']:,}")
    print(f"측정 시간: {analysis['duration']:.3f} 초")
    print(f"샘플링 레이트: {analysis['sampling_rate']:.2f} Hz")
    print(f"\nX축 통계:")
    print(f"  평균: {analysis['X']['mean']:.6f}")
    print(f"  표준편차: {analysis['X']['std']:.6f}")
    print(f"  최소값: {analysis['X']['min']:.6f}")
    print(f"  최대값: {analysis['X']['max']:.6f}")
    print(f"  범위: {analysis['X']['range']:.6f}")
    print(f"\nY축 통계:")
    print(f"  평균: {analysis['Y']['mean']:.6f}")
    print(f"  표준편차: {analysis['Y']['std']:.6f}")
    print(f"  최소값: {analysis['Y']['min']:.6f}")
    print(f"  최대값: {analysis['Y']['max']:.6f}")
    print(f"  범위: {analysis['Y']['range']:.6f}")
    print(f"\nZ축 통계:")
    print(f"  평균: {analysis['Z']['mean']:.6f}")
    print(f"  표준편차: {analysis['Z']['std']:.6f}")
    print(f"  최소값: {analysis['Z']['min']:.6f}")
    print(f"  최대값: {analysis['Z']['max']:.6f}")
    print(f"  범위: {analysis['Z']['range']:.6f}")
    print(f"\n힘 크기 (Magnitude) 통계:")
    print(f"  평균: {analysis['magnitude']['mean']:.6f}")
    print(f"  표준편차: {analysis['magnitude']['std']:.6f}")
    print(f"  최대값: {analysis['magnitude']['max']:.6f}")

def plot_time_series(df, filename, output_dir):
    """시계열 그래프를 생성합니다."""
    fig, axes = plt.subplots(4, 1, figsize=(12, 10))
    fig.suptitle(f'시계열 분석: {filename}', fontsize=14, fontweight='bold')
    
    # X, Y, Z 개별 플롯
    axes[0].plot(df['elapsed_time'], df['X'], label='X', linewidth=0.5)
    axes[0].set_ylabel('X 값')
    axes[0].set_title('X축 힘/토크')
    axes[0].grid(True, alpha=0.3)
    axes[0].legend()
    
    axes[1].plot(df['elapsed_time'], df['Y'], label='Y', color='orange', linewidth=0.5)
    axes[1].set_ylabel('Y 값')
    axes[1].set_title('Y축 힘/토크')
    axes[1].grid(True, alpha=0.3)
    axes[1].legend()
    
    axes[2].plot(df['elapsed_time'], df['Z'], label='Z', color='green', linewidth=0.5)
    axes[2].set_ylabel('Z 값')
    axes[2].set_title('Z축 힘/토크')
    axes[2].grid(True, alpha=0.3)
    axes[2].legend()
    
    # 힘 크기
    magnitude = np.sqrt(df['X']**2 + df['Y']**2 + df['Z']**2)
    axes[3].plot(df['elapsed_time'], magnitude, label='Magnitude', color='red', linewidth=0.5)
    axes[3].set_ylabel('힘 크기')
    axes[3].set_xlabel('경과 시간 (초)')
    axes[3].set_title('총 힘 크기')
    axes[3].grid(True, alpha=0.3)
    axes[3].legend()
    
    plt.tight_layout()
    output_path = os.path.join(output_dir, f'{filename}_timeseries.png')
    plt.savefig(output_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"시계열 그래프 저장: {output_path}")

def plot_histogram(df, filename, output_dir):
    """히스토그램을 생성합니다."""
    fig, axes = plt.subplots(2, 2, figsize=(12, 10))
    fig.suptitle(f'분포 분석: {filename}', fontsize=14, fontweight='bold')
    
    axes[0, 0].hist(df['X'], bins=50, alpha=0.7, edgecolor='black')
    axes[0, 0].set_xlabel('X 값')
    axes[0, 0].set_ylabel('빈도')
    axes[0, 0].set_title('X축 분포')
    axes[0, 0].grid(True, alpha=0.3)
    
    axes[0, 1].hist(df['Y'], bins=50, alpha=0.7, color='orange', edgecolor='black')
    axes[0, 1].set_xlabel('Y 값')
    axes[0, 1].set_ylabel('빈도')
    axes[0, 1].set_title('Y축 분포')
    axes[0, 1].grid(True, alpha=0.3)
    
    axes[1, 0].hist(df['Z'], bins=50, alpha=0.7, color='green', edgecolor='black')
    axes[1, 0].set_xlabel('Z 값')
    axes[1, 0].set_ylabel('빈도')
    axes[1, 0].set_title('Z축 분포')
    axes[1, 0].grid(True, alpha=0.3)
    
    magnitude = np.sqrt(df['X']**2 + df['Y']**2 + df['Z']**2)
    axes[1, 1].hist(magnitude, bins=50, alpha=0.7, color='red', edgecolor='black')
    axes[1, 1].set_xlabel('힘 크기')
    axes[1, 1].set_ylabel('빈도')
    axes[1, 1].set_title('힘 크기 분포')
    axes[1, 1].grid(True, alpha=0.3)
    
    plt.tight_layout()
    output_path = os.path.join(output_dir, f'{filename}_histogram.png')
    plt.savefig(output_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"히스토그램 저장: {output_path}")

def plot_comparison(all_analyses, output_dir):
    """모든 파일을 비교하는 그래프를 생성합니다."""
    filenames = [a['filename'] for a in all_analyses]
    
    # 힘 크기 최대값 비교
    fig, axes = plt.subplots(2, 2, figsize=(14, 10))
    fig.suptitle('파일 간 비교 분석', fontsize=14, fontweight='bold')
    
    max_magnitudes = [a['magnitude']['max'] for a in all_analyses]
    mean_magnitudes = [a['magnitude']['mean'] for a in all_analyses]
    
    axes[0, 0].bar(filenames, max_magnitudes, color='steelblue', alpha=0.7, edgecolor='black')
    axes[0, 0].set_ylabel('최대 힘 크기')
    axes[0, 0].set_title('최대 힘 크기 비교')
    axes[0, 0].tick_params(axis='x', rotation=45)
    axes[0, 0].grid(True, alpha=0.3, axis='y')
    
    axes[0, 1].bar(filenames, mean_magnitudes, color='coral', alpha=0.7, edgecolor='black')
    axes[0, 1].set_ylabel('평균 힘 크기')
    axes[0, 1].set_title('평균 힘 크기 비교')
    axes[0, 1].tick_params(axis='x', rotation=45)
    axes[0, 1].grid(True, alpha=0.3, axis='y')
    
    # Z축 범위 비교 (일반적으로 가장 큰 힘이 나타나는 축)
    z_ranges = [a['Z']['range'] for a in all_analyses]
    axes[1, 0].bar(filenames, z_ranges, color='green', alpha=0.7, edgecolor='black')
    axes[1, 0].set_ylabel('Z축 범위')
    axes[1, 0].set_title('Z축 힘 범위 비교')
    axes[1, 0].tick_params(axis='x', rotation=45)
    axes[1, 0].grid(True, alpha=0.3, axis='y')
    
    # 샘플링 레이트 비교
    sampling_rates = [a['sampling_rate'] for a in all_analyses]
    axes[1, 1].bar(filenames, sampling_rates, color='purple', alpha=0.7, edgecolor='black')
    axes[1, 1].set_ylabel('샘플링 레이트 (Hz)')
    axes[1, 1].set_title('샘플링 레이트 비교')
    axes[1, 1].tick_params(axis='x', rotation=45)
    axes[1, 1].grid(True, alpha=0.3, axis='y')
    
    plt.tight_layout()
    output_path = os.path.join(output_dir, 'comparison.png')
    plt.savefig(output_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"비교 그래프 저장: {output_path}")

def main():
    """메인 함수"""
    # 현재 스크립트 위치 기준으로 데이터 디렉토리 찾기
    script_dir = Path(__file__).parent
    data_dir = script_dir
    
    # 출력 디렉토리 생성
    output_dir = script_dir / 'analysis_output'
    output_dir.mkdir(exist_ok=True)
    
    # CSV 파일 찾기
    csv_files = list(data_dir.glob('*.csv'))
    
    if not csv_files:
        print("CSV 파일을 찾을 수 없습니다.")
        return
    
    print(f"총 {len(csv_files)}개의 CSV 파일을 찾았습니다.\n")
    
    all_analyses = []
    all_dataframes = {}
    
    # 각 파일 분석
    for csv_file in csv_files:
        print(f"\n분석 중: {csv_file.name}")
        result = analyze_file(csv_file)
        
        if result is None:
            continue
        
        analysis, df = result
        all_analyses.append(analysis)
        all_dataframes[csv_file.name] = df
        
        # 통계 출력
        print_analysis(analysis)
        
        # 그래프 생성
        plot_time_series(df, csv_file.stem, output_dir)
        plot_histogram(df, csv_file.stem, output_dir)
    
    # 비교 그래프 생성
    if len(all_analyses) > 1:
        print(f"\n{'='*60}")
        print("파일 간 비교 분석")
        print(f"{'='*60}")
        plot_comparison(all_analyses, output_dir)
        
        # 요약 테이블 생성
        summary_data = []
        for a in all_analyses:
            summary_data.append({
                '파일명': a['filename'],
                '샘플 수': a['total_samples'],
                '측정 시간(초)': f"{a['duration']:.3f}",
                '샘플링 레이트(Hz)': f"{a['sampling_rate']:.2f}",
                '최대 힘 크기': f"{a['magnitude']['max']:.6f}",
                '평균 힘 크기': f"{a['magnitude']['mean']:.6f}",
                'Z축 범위': f"{a['Z']['range']:.6f}"
            })
        
        summary_df = pd.DataFrame(summary_data)
        summary_path = output_dir / 'summary.csv'
        summary_df.to_csv(summary_path, index=False, encoding='utf-8-sig')
        print(f"\n요약 테이블 저장: {summary_path}")
        print("\n요약:")
        print(summary_df.to_string(index=False))
    
    print(f"\n{'='*60}")
    print(f"분석 완료! 결과는 {output_dir} 디렉토리에 저장되었습니다.")
    print(f"{'='*60}")

if __name__ == "__main__":
    main()

